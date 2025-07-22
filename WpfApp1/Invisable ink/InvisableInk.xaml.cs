using Microsoft.Win32;
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
using System.IO;
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
        bool isMessageBoxContainPath = false;
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
            appController.FileReceived += ShowReceivedFile;
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
            //if (isMessageBoxContainPath)
            //{
            //    MessageBox.Show("please press Send File button");
            //    return;
            //}
            string message = IMessageBox.Text.Trim();
            if (!string.IsNullOrEmpty(message) || !isUserContacted)
            {
                string targetedUser = UserTextbox.Text;
                if (!string.IsNullOrEmpty(targetedUser))
                {
                    await appController.SendMessage(message, targetedUser);
                    AppendChat($"You: {message}");
                    
                    IMessageBox.Clear();
                }
                else
                    System.Windows.MessageBox.Show("Client : Please enter Username of user you want");
            }
        }
        private void AppendChat(string message)
        {
            TextBlock textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 400
            };
            Dispatcher.Invoke(() =>
            {
                ChatBox.Items.Add(textBlock);
                ChatBox.ScrollIntoView(ChatBox.Items[ChatBox.Items.Count - 1]);
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

        private async void SendFileButton_Click(Object sender, RoutedEventArgs e)
        {
            string targetedUser = UserTextbox.Text;
            if (!string.IsNullOrEmpty(targetedUser))
            {
                if (isMessageBoxContainPath)
                {
                    await appController.SendFile(IMessageBox.Text , targetedUser);
                    AppendChat($"You: {IMessageBox.Text}");
                    IMessageBox.Clear();
                    isMessageBoxContainPath = false;
                    IMessageBox.IsReadOnly = false;
                    SendButton.IsEnabled = true;
                }
                else
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Text documents (.txt)|*.txt|All files (*.*)|*.*";
                    Nullable<bool> result = openFileDialog.ShowDialog();

                    if (result == true)
                    {
                        string filename = openFileDialog.FileName;
                        IMessageBox.Text = filename;
                        IMessageBox.IsReadOnly = true;
                        isMessageBoxContainPath = true;
                        SendButton.IsEnabled = false;
                    }
                }
            }
            else
                MessageBox.Show("Client : Please enter Username of user you want");
        }
        private void AddFileToChat(string fileName , long Size, byte[] fileData)
        {
            Dispatcher.Invoke(() =>
            {
                StackPanel fileContainer = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };

                // File Icon
                Image icon = new Image
                {
                    Source = new BitmapImage(new Uri("D:\\01 Kareem\\programing projects\\VirtualOS\\WpfApp1\\Assets\\icons\\Paomedia-Small-N-Flat-File-text.ico")),
                    Width = 24,
                    Height = 24,
                    Margin = new Thickness(5, 0, 5, 0)
                };

                // File Name Text
                TextBlock fileNameText = new TextBlock
                {
                    Text = $"{fileName}\nSize:{Size}",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0)
                };

                // Download Button
                Button downloadButton = new Button
                {
                    Content = "Download",
                    Tag = (fileName , Size, fileData),
                    VerticalAlignment = VerticalAlignment.Center,
                    IsEnabled = true,
                    TabIndex = 1
                };
                
                downloadButton.Click += DownloadFile_Click;

                fileContainer.Children.Add(icon);
                fileContainer.Children.Add(fileNameText);
                fileContainer.Children.Add(downloadButton);

                ChatBox.Items.Add(fileContainer);
            });
        }

        private void DownloadFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ValueTuple<string , long, byte[]> fileInfo)
            {
                string fileName = fileInfo.Item1;
                byte[] fileData = fileInfo.Item3;

                // Ask user where to save
                Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = fileName,
                    Filter = "All files (*.*)|*.*"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(saveDialog.FileName, fileData);
                    MessageBox.Show($"File saved to: {saveDialog.FileName}");
                }
            }
        }

        private void ShowReceivedFile(string fileName , long Size , byte[] fileData)
        {
            Dispatcher.Invoke(() =>
            {
                AddFileToChat(fileName, Size, fileData);
            });
        }
    }
}
