using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace VirtualOS.Commuication
{
    public class BaseMessageType
    {
        public string Type { get; set; }
    }
    public class Server
    {
        private TcpListener listener;
        private Dictionary<string, Client> clients = new Dictionary<string, Client>();
        public const int PORT = 5000;

        public Server()
        {
            listener = new TcpListener(IPAddress.Any, PORT);
            _ = StartListening();
        }
        private async Task StartListening()
        {
            listener.Start();
            while (true)
            {
                TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                NetworkStream stream = tcpClient.GetStream();

                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string userName = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                var newClient = new Client(userName, tcpClient);
                clients[userName] = newClient;

                _ = HandleClientAsync(newClient);
            }
        }
        private async Task HandleClientAsync(Client senderClient)
        {
            NetworkStream stream = senderClient.GetClientStream();
            byte[] buffer = new byte[4096];

            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                byte[] actualData = buffer.Take(bytesRead).ToArray();
                string json = Encoding.UTF8.GetString(actualData);

                BaseMessageType baseType;
                try
                {
                    baseType = JsonSerializer.Deserialize<BaseMessageType>(json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Server : Failed to parse message type: {ex.Message}");
                    continue;
                }

                if (baseType == null || string.IsNullOrEmpty(baseType.Type))
                    continue;

                switch (baseType.Type)
                {
                    case "SessionRequest":
                        var sessionRequest = JsonSerializer.Deserialize<SessionRequest>(json);
                        await HandleSessionRequest(sessionRequest);
                        break;

                    case "SessionKeyResponse":
                        var sessionKeyResponse = JsonSerializer.Deserialize<SessionKeyResponse>(json);
                        await ForwardSessionKeyResponse(sessionKeyResponse);
                        break;

                    case "Message":
                        var message = JsonSerializer.Deserialize<Message>(json);
                        if (clients.TryGetValue(message.TargetUser, out Client targetClient))
                        {
                            byte[] forwardBytes = Encoding.UTF8.GetBytes(json);
                            await targetClient.GetClientStream().WriteAsync(forwardBytes, 0, forwardBytes.Length);
                        }
                        break;

                    default:
                        MessageBox.Show($"Server : Unknown message type: {baseType.Type}");
                        break;
                }
            }
        }
        private async Task ForwardSessionKeyResponse(SessionKeyResponse response)
        {
            if (clients.TryGetValue(response.TargetUser, out Client targetClient))
            {
                byte[] data = IResponse.SerializeResponse(response);
                await targetClient.GetClientStream().WriteAsync(data, 0, data.Length);
            }
        }
        private async Task HandleSessionRequest(SessionRequest request)
        {
            if (clients.TryGetValue(request.TargetUser, out Client targetClient))
            {
                byte[] forwardBytes = IRequest.SerializeRequest(request);
                await targetClient.GetClientStream().WriteAsync(forwardBytes, 0, forwardBytes.Length);
            }
        }
        public void ChangeAUserNameKey(string oldName, string newName)
        {
            if (clients.TryGetValue(oldName, out Client client) && !clients.ContainsKey(newName))
            {
                clients.Remove(oldName);
                client.putUserName(newName);
                clients.Add(newName, client);
            }
            else
            {
                MessageBox.Show($"Server: Cannot change username. Either '{oldName}' doesn't exist or '{newName}' is already taken.");
            }
        }
    }

}
