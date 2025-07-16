using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text.Json;
namespace VirtualOS.Commuication
{

    [Serializable]
    public struct Message
    {
        public string Sender { get; set; }
        public string TargetUser { get; set; }
        public string Content { get; set; }
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
    
    public class Client : TcpClient
    {
        public string userName;
        private string SERVER_IP;
        private int SERVER_PORT;
        private NetworkStream stream;
        public event Action<string> MessageReceived;
        public Client(string _userName, string ip, int port)
        {
            userName = _userName;
            SERVER_IP = ip;
            SERVER_PORT = port;
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
                //decryption session key
                try
                {
                    var messageObj = JsonSerializer.Deserialize<Message>(jsonMessage);
                    string formatted = $"{messageObj.Sender} : {messageObj.Content}";
                    MessageReceived?.Invoke(formatted);
                }
                catch (Exception ex)
                {
                    // Optionally log or handle invalid message formats
                    Console.WriteLine($"Deserialization error: {ex.Message}");
                }
            }
        }
        public NetworkStream GetClientStream()
        {
            return stream;
        }

        public async Task SendMessage(string targetuser , string content)
        {
            Message message = new Message {
                Sender = userName,
                TargetUser = targetuser,
                Content = content
            };
            byte[] messageBytes = IMessage.SerializeMessage(message);
            //encrypt with session key
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
        }
    }
}
