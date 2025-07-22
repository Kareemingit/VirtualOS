using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualOS.Commuication;
using System.Windows;
using System.Windows.Controls;

namespace VirtualOS
{
    public abstract class VOSCore
    {
    }

    public class FileManager : VOSCore
    {
        private VirtualDirController virtualDirController = VirtualDirController.Instance;
        private List<VirtualDir> CurrentPathNodes = new();
        private VirtualDir? currentUserNode;
        private int currentUserNodeIndex = -1;
        public void Load()
        {
            virtualDirController.StartLoader();
        }
        public DeskDriver GetRoot()
        {
            return virtualDirController.driverTree;
        }
        public void RunRoot()
        {
            currentUserNodeIndex++;
            currentUserNode = virtualDirController.driverTree;
            CurrentPathNodes.Add(currentUserNode);
        }
        public VirtualDir BackTrack()
        {
            if (currentUserNodeIndex == 0) return null;
            currentUserNodeIndex--;
            currentUserNode = CurrentPathNodes[currentUserNodeIndex];
            return currentUserNode;
        }
        public VirtualDir GoForward()
        {
            if (currentUserNodeIndex == CurrentPathNodes.Count - 1) return null;
            currentUserNodeIndex++;
            currentUserNode = CurrentPathNodes[currentUserNodeIndex];
            return currentUserNode;
        }
        public void OpenNewFolder(VirtualFolder folder)
        {
            if (folder == null) return;
            if (CurrentPathNodes.Count > currentUserNodeIndex + 1)
            {
                CurrentPathNodes.RemoveRange(currentUserNodeIndex + 1, CurrentPathNodes.Count - (currentUserNodeIndex + 1));
            }
            CurrentPathNodes.Add(folder);
            //folder.Open(folder.Virtualpath);
            currentUserNode = folder;
            currentUserNodeIndex++;
        }
        public void OpenNew(VirtualDir vDir)
        {
            if (vDir == null) return;

            if (vDir is VirtualFolder folder)
            {
                OpenNewFolder(folder);
            }
            if (vDir is VirtualFile)
            {

            }
        }
        public void CreateNewFolder(string folderName , VirtualDir distinationNode)
        {
            string newuipath = distinationNode.UIPath + '/' + folderName;
            string newvirtualpath = distinationNode.Virtualpath + "\\" + folderName;
            VirtualFolder newFolder = new VirtualFolder(folderName, newvirtualpath ,newuipath);
            virtualDirController.PlantNewItem(newFolder ,distinationNode);
        }
        public void CreateNewFile(string fileName , VirtualDir distinationNode)
        {
            VirtualFile newFile = null;
            string newuipth = distinationNode.UIPath + '/' + fileName;
            string newvirtualpth = distinationNode.Virtualpath + "\\" + fileName;
            if (Path.GetExtension(fileName) == ".txt")
            {
                newFile = new TXTFile(fileName, newvirtualpth, newuipth);
            }
            
            else {
                newFile = new UnSupportedFile(fileName , newvirtualpth , ".txt" , newuipth);
            }
            virtualDirController.PlantNewItem(newFile ,distinationNode);
        }
        public void RenameItem(VirtualDir vDir , string newName)
        {
            vDir.Name = newName;
        }
        public void DeleteNode(VirtualDir target)
        {
            if(target == null) return;
            if (target is VirtualFolder folder){
                virtualDirController.DeleteFolder(folder);
            }
            else if(target is VirtualFile file){
                virtualDirController.DeleteFile(file);
            }
        }
    }

    
    public class AppController
    {
        private Client client;
        private static Server? serverinstanc;
        public event Action<string> MessageReceived;
        public event Action<string , long , byte[]> FileReceived;
        public AppController()
        {
            
        }
        public void StartServer()
        {
            if(serverinstanc == null)
                serverinstanc = new Server();
        }
        public void SetClient(Client _client)
        {
            client = _client;
            client.MessageReceived += OnMessageReceived;
            client.FileReceived += OnFileReceived;
            _ = client.ListenForMessages();
        }
        private void OnFileReceived(string fileName , long Size , byte[] fileData)
        {
            FileReceived?.Invoke(fileName , Size, fileData);
        }
        private void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(message); // Propagate to UI
        }
        public void ChangeUserName(string newName , string oldName)
        {
            serverinstanc.ChangeAUserNameKey(oldName, newName);
            client.putUserName(newName);
        }
        public async Task SendMessage(string message, string targetUser)
        {
            await client.SendMessage(targetUser , message);
        }
        public async Task SetUpSession(string targetuser)
        {
            await client.SendSessionRequest(targetuser);
        }
        public async Task SendFile(string Filepath , string target)
        {
            await client.SendFile(target, Filepath);
        }
    }
}

