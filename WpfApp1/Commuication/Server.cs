using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net;
using System.Windows;

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

                var msg = IMessage.DeserializeMessage(buffer.Take(bytesRead).ToArray());
                if (clients.TryGetValue(msg.TargetUser, out Client targetClient))
                {
                    byte[] forwardBytes = IMessage.SerializeMessage(msg);
                    await targetClient.GetClientStream().WriteAsync(forwardBytes, 0, forwardBytes.Length);
                }
            }
        }


        public void ChangeAUserNameKey(string oldName, string newName)
        {
            if (clients.TryGetValue(oldName, out Client client) && !clients.ContainsKey(newName))
            {
                clients.Remove(oldName);
                client.putUserName(newName); // update username in Client
                clients.Add(newName, client);
            }
            else
            {
                MessageBox.Show($"Server: Cannot change username. Either '{oldName}' doesn't exist or '{newName}' is already taken.");
            }
        }
    }

}





/*
        internal class Server
        {
            private TcpListener listener;
            private Dictionary<string , Client> clients = new Dictionary<string , Client>();
            public const int PORT = 5000;
            public Server()
            {
                listener = new TcpListener(IPAddress.Any , PORT);
                _ = StartListening();
            }

            private async Task StartListening()
            {
                listener.Start();
                while (true)
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                    NetworkStream stream = tcpClient.GetStream();

                    // Receive username immediately
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string userName = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Create the Client instance by wrapping TcpClient
                    Client newClient = new Client(userName, tcpClient);

                    if (!clients.ContainsKey(userName))
                    {
                        clients.Add(userName, newClient);
                        _ = HandleUserAsync(newClient);
                    }
                    else
                    {
                        MessageBox.Show($"Server: Username {userName} already exists.");
                        tcpClient.Close();
                    }
                }
            }

            private async Task HandleUserAsync(Client senderClient)
            {
                NetworkStream stream = senderClient.GetClientStream();
                byte[] buffer = new byte[1024];

                try
                {
                    while (true)
                    {
                        int byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (byteCount == 0) break;

                        string fullMessage = Encoding.UTF8.GetString(buffer, 0, byteCount);
                        // Expect message format: targetUser:actualMessage
                        var parts = fullMessage.Split(':', 2);
                        if (parts.Length == 2)
                        {
                            string targetUser = parts[0].Trim();
                            string messageContent = parts[1].Trim();

                            if (clients.TryGetValue(targetUser, out Client targetClient))
                            {
                                byte[] messageBytes = Encoding.UTF8.GetBytes($"{senderClient.userName}: {messageContent}");
                                await targetClient.GetClientStream().WriteAsync(messageBytes, 0, messageBytes.Length);
                            }
                            else
                            {
                                string errorMsg = $"Server: User {targetUser} does not exist.";
                                byte[] errorBytes = Encoding.UTF8.GetBytes(errorMsg);
                                await senderClient.GetClientStream().WriteAsync(errorBytes, 0, errorBytes.Length);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Server error: {ex.Message}");
                }
                finally
                {
                    senderClient.Close();
                }
            }

            public void ChangeAUserNameKey(string oldName, string newName)
            {
                bool isoldNameexist = clients.ContainsKey(oldName);
                bool isNewnamedoesnotexist = !clients.ContainsKey(newName);
                if (isoldNameexist && isNewnamedoesnotexist)
                {
                    Client client = clients[oldName];
                    clients.Remove(oldName);
                    clients.Add(newName, client);
                }
                else
                {
                    MessageBox.Show($"Server: Cannot change username. Either '{oldName}' doesn't exist or '{newName}' is already taken.");
                }
            }
        }

       */