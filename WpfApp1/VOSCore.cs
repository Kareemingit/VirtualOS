using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public void RenameItem(VirtualDir vDir , string newName)
        {

        }
        public void DeleteNode(VirtualDir target)
        {
            if(target == null) return;
            if (target is VirtualFolder folder)
            {
                virtualDirController.DeleteFolder(folder);
            }
            else
            {

            }
        }
    }
}
