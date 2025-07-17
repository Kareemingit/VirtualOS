using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;
namespace VirtualOS.Commuication
{

    [Serializable]
    public struct Message
    {
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
    
    public class Client : TcpClient
    {
        public string userName;
        private string SERVER_IP;
        private int SERVER_PORT;
        private NetworkStream stream;
        private KeyManager keyManager;
        private Dictionary<string, string> CurrentOpenSessions = new Dictionary<string, string>();
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
            SetRequestKeys();
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
        private void SetRequestKeys()
        {
            keyManager.GenerateRSAKeys();
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

                string jsonMessage = Encoding.UTF8.GetString(buffer, 0, bufferCount);
                
                try
                {
                    var messageObj = JsonSerializer.Deserialize<Message>(jsonMessage);
                    //decrypt messageObj.Content session key
                    string formatted = $"{messageObj.Sender} : {messageObj.Content}";
                    MessageReceived?.Invoke(formatted);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Deserialization error: {ex.Message}");
                }
            }
        }

        public async Task ListenForSessionRequest()
        {
            byte[] buffer = new byte[1024];

            while (true)
            {
                int bufferCount = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bufferCount == 0) break;

                string jsonRequest = Encoding.UTF8.GetString(buffer, 0, bufferCount);
                try
                {
                    var ReqstObj = IRequest.DeserializeRequest(buffer);
                    if (ReqstObj.Type == "SessionRequest")
                    {
                        await AcceptSessionRequest(ReqstObj);
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Client Request : {ex.Message}");
                }
            }
        }
        public NetworkStream GetClientStream()
        {
            return stream;
        }
        public async Task SendMessage(string targetuser , string content)
        {
            //encrypt content with session key
            Message message = new Message {
                Sender = userName,
                TargetUser = targetuser,
                Content = content
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

        }
    }
}
