using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace VirtualOS.Commuication
{
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
                var rqst = IRequest.DeserializeRequest(buffer);
                if(rqst.Type == "SessionRequest")
                {
                    await HandleSessionRequest(rqst);
                }
                var msg = IMessage.DeserializeMessage(buffer.Take(bytesRead).ToArray());
                if (clients.TryGetValue(msg.TargetUser, out Client targetClient))
                {
                    byte[] forwardBytes = IMessage.SerializeMessage(msg);
                    await targetClient.GetClientStream().WriteAsync(forwardBytes, 0, forwardBytes.Length);
                }
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
