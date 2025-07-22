using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace VirtualOS.Commuication
{
    [Serializable]
    public struct Message
    {
        public string Type { get; set; }
        public string Sender { get; set; }
        public string TargetUser { get; set; }
        public string Content { get; set; }
    }
    public struct SessionRequest
    {
        public string Type { get; set; }
        public string Sender { get; set; }
        public string TargetUser { get; set; }
        public string PublicKey { get; set; }
    }
    public struct SessionKeyResponse
    {
        public string Type { get; set; }
        public string Sender { get; set; }
        public string TargetUser { get; set; }
        public byte[] EncryptedKey { get; set; }
        public byte[] EncryptedIv { get; set; }
    }
    public struct MetaData
    {
        public string Type { get; set; }
        public string FileName { get; set; }
        public string Sender { get; set; }
        public string TargetUser { get; set; }
        public long FileLength { get; set; }
    }
    public struct FileDataCarrier
    {
        public string Type { get; set; }
        public string Sender { get; set; }
        public string TargetUser { get; set; }
        public byte[] Data { get; set; }
    }
    public class SessionData
    {
        public byte[] Key { get; set; }
        public byte[] Iv { get; set; }
    }
    public static class IMessage
    {
        public static byte[] SerializeMessage(Message message)
        {
            string json = JsonSerializer.Serialize(message);
            return Encoding.UTF8.GetBytes(json);
        }
        public static Message DeserializeMessage(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<Message>(json);
        }
    }
    public static class IRequest
    {
        public static byte[] SerializeRequest(SessionRequest request)
        {
            string json = JsonSerializer.Serialize(request);
            return Encoding.UTF8.GetBytes(json);
        }
        public static SessionRequest DeserializeRequest(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<SessionRequest>(json);
        }
    }
    public static class IResponse
    {
        public static byte[] SerializeResponse(SessionKeyResponse response)
        {
            string json = JsonSerializer.Serialize(response);
            return Encoding.UTF8.GetBytes(json);
        }
        public static SessionKeyResponse DeserializeRequest(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<SessionKeyResponse>(json);
        }
    }
    public static class IFile
    {
        public static byte[] SerializeFile(FileDataCarrier fileDataCarrier)
        {
            string json = JsonSerializer.Serialize(fileDataCarrier);
            return Encoding.UTF8.GetBytes(json);
        }
        public static FileDataCarrier DeserializeFile(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<FileDataCarrier>(json);
        }
    }
    public class Client : TcpClient
    {
        public string userName;
        public long MessageLength;
        private string SERVER_IP;
        private int SERVER_PORT;
        private NetworkStream stream;
        private KeyManager keyManager;
        private MetaData FileMessageMetaData;
        private Dictionary<string, SessionData> CurrentOpenSessions = new Dictionary<string, SessionData>();
        public event Action<string> MessageReceived;
        public event Action<string,long ,byte[]> FileReceived;

        public Client(string _userName, string ip, int port)
        {
            userName = _userName;
            SERVER_IP = ip;
            SERVER_PORT = port;
            keyManager = new KeyManager();
            ConnectToServer();
        }
        public Client(string _userName, TcpClient tcpClient) : base()
        {
            userName = _userName;
            stream = tcpClient.GetStream();
        }
        private async void ConnectToServer()
        {
            await ConnectAsync(SERVER_IP, SERVER_PORT);
            stream = GetStream();
            // Send username immediately after connecting
            byte[] userNameBytes = Encoding.UTF8.GetBytes(userName);
            await stream.WriteAsync(userNameBytes, 0, userNameBytes.Length);
        }
        public async Task SendSessionRequest(string target)
        {
            if (!CurrentOpenSessions.ContainsKey(target))
            {
                keyManager.GenerateRSAKeys();
                string pubKey = GetRequestPublicKey();
                var SessionRequest = new SessionRequest
                {
                    Type = "SessionRequest",
                    Sender = userName,
                    TargetUser = target,
                    PublicKey = pubKey
                };
                MessageLength = 4096;
                byte[] data = IRequest.SerializeRequest(SessionRequest);
                await stream.WriteAsync(data, 0, data.Length);
            }
        }
        public void putUserName(string newName)
        {
            userName = newName;
        }
        public async Task ListenForMessages()
        {
            while (stream == null)
            {
                await Task.Delay(50);
            }
            byte[] buffer = new byte[1024];
            
            while (true)
            {
                int bufferCount = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bufferCount == 0) break;
                byte[] actualData = buffer.Take(bufferCount).ToArray();

                string json = Encoding.UTF8.GetString(actualData);
                BaseMessageType messageType;
                try
                {
                    messageType = JsonSerializer.Deserialize<BaseMessageType>(json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Client : Failed to parse message type: {ex.Message}");
                    continue;
                }
                if (messageType == null || string.IsNullOrEmpty(messageType.Type))
                    continue;
                switch (messageType.Type)
                {
                    case "SessionRequest":
                        var reqst = JsonSerializer.Deserialize<SessionRequest>(json);
                        await AcceptSessionRequest(reqst);
                        break;

                    case "SessionKeyResponse":
                        var resp = JsonSerializer.Deserialize<SessionKeyResponse>(json);
                        ProcessSessionKeyResponse(resp);
                        break;

                    case "MetaData":
                        var md = JsonSerializer.Deserialize<MetaData>(json);
                        buffer = new byte[md.FileLength + 1024];
                        FileMessageMetaData = md;
                        break;

                    case "FileTransfer":
                        var file = JsonSerializer.Deserialize<FileDataCarrier>(json);
                        buffer = new byte[1024];
                        ProcessFileReceived(file);
                        break;
                    case "Message":
                        var messageObj = JsonSerializer.Deserialize<Message>(json);
                        //decrypt messageObj.Content session key
                        if (CurrentOpenSessions.TryGetValue(messageObj.Sender,out SessionData sessionData)) {
                            byte[] messageBytes = Convert.FromBase64String(messageObj.Content);
                            string decryptedMessage = Decrypt(messageBytes, sessionData.Key, sessionData.Iv);
                            string formatted = $"{messageObj.Sender} : {decryptedMessage}";
                            MessageReceived?.Invoke(formatted);
                        }
                        break;

                    default:
                        MessageBox.Show($"Client : Unknown message type: {messageType.Type}");
                        break;
                }
            }
        }
        private void ProcessFileReceived(FileDataCarrier file)
        {
            if (CurrentOpenSessions.TryGetValue(file.Sender, out SessionData session))
            {
                string decyptedMessage = Decrypt(file.Data, session.Key, session.Iv);
                byte[] decyptedMessagebytes = Encoding.UTF8.GetBytes(decyptedMessage);
                FileReceived?.Invoke(FileMessageMetaData.FileName, FileMessageMetaData.FileLength , decyptedMessagebytes);
            }
        }
        private string Decrypt(byte[] encryptedPassword, byte[] key, byte[] iv)
        {
            string simpletext = String.Empty;
            using (Aes aes = Aes.Create())
            {
                ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);
                using (MemoryStream memoryStream = new MemoryStream(encryptedPassword))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cryptoStream))
                            simpletext = reader.ReadToEnd();
                    }
                }
            }
            return simpletext;
        }
        private void ProcessSessionKeyResponse(SessionKeyResponse response)
        {
            byte[] sessionKey = DecryptWithPrivateKey(response.EncryptedKey);
            byte[] sessionIv = DecryptWithPrivateKey(response.EncryptedIv);

            string sessionKeyBase64 = Convert.ToBase64String(sessionKey);
            string sessionIvBase64 = Convert.ToBase64String(sessionIv);
            SessionData combined = new SessionData {
                Key = sessionKey,
                Iv = sessionIv
            };

            CurrentOpenSessions[response.Sender] = combined;
        }
        private byte[] DecryptWithPrivateKey(byte[] encryptedKey)
        {
            byte[] decryptedKey;
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(source: Convert.FromBase64String(keyManager.RSAprivateKey), out _);
                decryptedKey = rsa.Decrypt(encryptedKey, RSAEncryptionPadding.Pkcs1);
            }
            return decryptedKey;
        }
        public NetworkStream GetClientStream()
        {
            return stream;
        }
        private byte[] Encrypt(string message, byte[] key, byte[] iv)
        {
            byte[] cipheredText;
            using (Aes aes = Aes.Create())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(key, iv);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(message);
                        }
                        cipheredText = memoryStream.ToArray();
                    }
                }
            }
            return cipheredText;
        }
        public async Task SendMessage(string targetuser , string content)
        {
            string encryptedmessageText;
            //encrypt content with session key
            if (CurrentOpenSessions.TryGetValue(targetuser, out SessionData session))
            {
                byte[] encryptedmessage = Encrypt(content, session.Key, session.Iv);
                encryptedmessageText = Convert.ToBase64String(encryptedmessage);
            }
            else {
                MessageBox.Show("Client : Session not found");
                return;
            }
            Message message = new Message {
                Type = "Message",
                Sender = userName,
                TargetUser = targetuser,
                Content = encryptedmessageText
            };
            
            byte[] messageBytes = IMessage.SerializeMessage(message);
            
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
        }
        private async Task SendFileMetaData(MetaData metaData , string target)
        {
            string json = JsonSerializer.Serialize(metaData);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(bytes);
        }
        public async Task SendFile(string targetuser, string Filepath)
        {
            if (CurrentOpenSessions.TryGetValue(targetuser, out SessionData session))
            {
                FileInfo fileInfo = new FileInfo(Filepath);
                byte[] content = File.ReadAllBytes(Filepath);
                FileDataCarrier fileData = new FileDataCarrier
                {
                    Type = "FileTransfer",
                    Sender = userName,
                    TargetUser = targetuser,
                    Data = content
                };
                MetaData meta = new MetaData
                {
                    Type = "MetaData",
                    Sender = userName,
                    TargetUser = targetuser,
                    FileName = fileInfo.Name,
                    FileLength = IFile.SerializeFile(fileData).Length
                };
                _ = SendFileMetaData(meta, targetuser);

                string strContent = Encoding.UTF8.GetString(content);
                byte[] encryptedContent = Encrypt(strContent, session.Key, session.Iv);
                fileData.Data = encryptedContent;
                byte[] fileByts = IFile.SerializeFile(fileData);
                await stream.WriteAsync(fileByts, 0, fileByts.Length);
            }
            else
            {
                MessageBox.Show("Client : Session not found");
                return;
            }
        }
        private string GetRequestPublicKey()
        {
            string pubKey = keyManager.RSApublicKey;
            if (pubKey != null)
                return pubKey;
            return null;
        }
        private async Task AcceptSessionRequest(SessionRequest request)
        {
            keyManager.GenerateAESSessionKeyAndIv();
            byte[][] key = keyManager.AESSessionkey;
            byte[] encryptedKey = EncryptSessionKey(request.PublicKey, key[0]);
            byte[] encryptedIv = EncryptSessionIv(request.PublicKey, key[1]);
            byte[][] fullKey = { encryptedKey, encryptedIv };

            var keyResponse = new SessionKeyResponse
            {
                Type = "SessionKeyResponse",
                Sender = userName,
                TargetUser = request.Sender,
                EncryptedKey = encryptedKey,
                EncryptedIv = encryptedIv
            };

            byte[] responseBytes = IResponse.SerializeResponse(keyResponse);
            CurrentOpenSessions[request.Sender] = new SessionData { 
                Key = key[0],
                Iv = key[1]
            };
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }
        private byte[] EncryptSessionKey(string publicKey , byte[] key)
        {
            byte[] encryptedKey;
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(source: Convert.FromBase64String(publicKey), out _);
                encryptedKey = rsa.Encrypt(key, RSAEncryptionPadding.Pkcs1);
            }
            return encryptedKey;
        }
        private byte[] EncryptSessionIv(string publicKey, byte[] iv)
        {
            byte[] encryptedIv;
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(source: Convert.FromBase64String(publicKey), out _);
                encryptedIv = rsa.Encrypt(iv, RSAEncryptionPadding.Pkcs1);
            }
            return encryptedIv;
        }
    }
}
