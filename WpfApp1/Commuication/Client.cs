using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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
    
    public class Client : TcpClient
    {
        public string userName;
        private string SERVER_IP;
        private int SERVER_PORT;
        private NetworkStream stream;
        private KeyManager keyManager;
        private Dictionary<string, SessionData> CurrentOpenSessions = new Dictionary<string, SessionData>();
        public event Action<string> MessageReceived;

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
