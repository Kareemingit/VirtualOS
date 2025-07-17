using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VirtualOS.Commuication;

namespace VirtualOS.Invisable_ink
{
    /// <summary>
    /// Interaction logic for InvisableInk.xaml
    /// </summary>
    ///

    public partial class InvisableInk : Window
    {
        AppController appController;
        Client UIOwner;
        string username;
        static int windowsCount = 0;
        bool isUserContacted = false;
        public InvisableInk()
        {
            InitializeComponent();
            appController = new AppController();
            appController.StartServer();
            username = GeneratInitialName();
            YourUserNameTextBox.Text = username;
            UIOwner = new Client(username, "127.0.0.1", 5000);
            appController.SetClient(UIOwner);
            appController.MessageReceived += AppendChat;
            windowsCount++;
        }
        private string GeneratInitialName()
        {
            return $"User_{windowsCount}";
        }
        private void ChangeB_Click(object sender, RoutedEventArgs e)
        {
            string newUserName = YourUserNameTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(newUserName))
            {
                appController.ChangeUserName(newUserName , username);
                System.Windows.MessageBox.Show($"Username changed to {newUserName}");
                username = newUserName;
            }
            else
            {
                System.Windows.MessageBox.Show("Please enter a new username.");
            }
        }
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageBox.Text.Trim();
            if (!string.IsNullOrEmpty(message) || !isUserContacted)
            {
                string targetedUser = UserTextbox.Text;
                if (!string.IsNullOrEmpty(targetedUser))
                {
                    await appController.SendMessage(message, targetedUser);
                    AppendChat($"You: {message}");
                    MessageBox.Clear();
                }
                else
                    System.Windows.MessageBox.Show("Client : Please enter Username of user you want");
            }
        }
        private void AppendChat(string message)
        {
            Dispatcher.Invoke(() =>
            {
                ChatBox.AppendText(message + "\n");
                ChatBox.ScrollToEnd();
            });
        }

        private async void Contact_Click(object sender, RoutedEventArgs e)
        {
            isUserContacted = true;
            string targetedUser = UserTextbox.Text;
            if (!string.IsNullOrEmpty(targetedUser))
                await appController.SetUpSession(targetedUser);
        }

        private void Change_Peer(object sender, TextChangedEventArgs e)
        {
            isUserContacted = false;
        }
    }
}
