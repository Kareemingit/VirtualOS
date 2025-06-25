using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace VirtualOS
{
    //public class Properties
    //{
    //    public int CPUusage;
    //    public double usedSpaceByts;
    //    public double usedSpaceUn;
    //    public double freeSpaceByts;
    //    public double freeSpaceUn;
    //    public int FilesCount;
    //    public int FoldersCount;
    //}

    public class DeskDriver
    {
        private const string ActualPath = "D:/01 Kareem/programing projects/VirtualOS/WpfApp1/Root Desk";
        public string? Name;
        public List<VirtualDir> Children;

        public DeskDriver(string _name , string _vpath)
        {
            Name = _name;
        }


        private List<VirtualDir> BuildTree(DirectoryInfo dir)
        {
            var list = new List<VirtualDir>();

            // Add directories
            foreach (var subDir in dir.GetDirectories())
            {
                var vFolder = new VirtualFolder(subDir.Name, subDir.FullName)
                {
                    Children = BuildTree(subDir)
                };
                list.Add(vFolder);
            }

            // Add files
            foreach (var file in dir.GetFiles())
            {
                
                VirtualFile? vFile = null;
                if (file.Extension == ".txt")
                {
                    vFile = new TXTFile(file.Name, file.FullName);
                }
                else
                {
                    vFile = new VirtualFile(file.Name, file.FullName, file.Extension);
                }

                list.Add(vFile);
            }

            return list;
        }

        public void loadDeskTreeV1()
        {
            var rootInfo = new DirectoryInfo(ActualPath);
            foreach(var item in BuildTree(rootInfo))
            {
                Children.Add(item);
            }
        }

        private IEnumerable<DirectoryInfo> SafeGetDirectories(DirectoryInfo dir)
        {
            try { return dir.GetDirectories(); }
            catch { return Array.Empty<DirectoryInfo>(); }
        }

        private IEnumerable<FileInfo> SafeGetFiles(DirectoryInfo dir)
        {
            try { return dir.GetFiles(); }
            catch { return Array.Empty<FileInfo>(); }
        }

        public void loadDeskTreeV2()
        {
            var rootInfo = new DirectoryInfo(ActualPath);

            var rootFolder = new VirtualFolder(rootInfo.Name , rootInfo.FullName)
            {
                Children = new List<VirtualDir>()
            };

            Stack<(DirectoryInfo dirInfo, VirtualFolder parent)> stack = new();
            stack.Push((rootInfo, rootFolder));

            while (stack.Count > 0)
            {
                var (currentDirInfo, currentVirtualFolder) = stack.Pop();

                // Add subdirectories
                foreach (var subDir in SafeGetDirectories(currentDirInfo))
                {
                    var subFolder = new VirtualFolder(subDir.Name , subDir.FullName)
                    {
                        Children = new List<VirtualDir>()
                    };

                    currentVirtualFolder.Children.Add(subFolder);
                    stack.Push((subDir, subFolder)); // Push for further traversal
                }

                // Add files
                foreach (var file in SafeGetFiles(currentDirInfo))
                {
                    VirtualFile? vFile = null;

                    if(file.Extension == ".txt")
                    {
                        vFile = new TXTFile(file.Name, file.FullName);
                    }
                    else
                    {
                        vFile = new VirtualFile(file.Name, file.FullName, file.Extension);
                    }
                    currentVirtualFolder.Children.Add(vFile);
                }
            }

            // Attach root folder to DeskDriver
            Children = rootFolder.Children;
        }
        public void Open()
        {

        }
        public void AddChild(VirtualDir child)
        {

        }
        public void DeleteChild(VirtualDir child)
        {

        }
        public void Close()
        {

        }
    }

    public abstract class VirtualDir
    {
        public string? Name;
        public string? Virtualpath;

        public VirtualDir(string? name, string? virtualpath)
        {
            Name = name;
            Virtualpath = virtualpath;
        }

        public virtual void Create(string Vpath) 
        {

        }
        public virtual void Open(string Vpath)
        {

        }

        public virtual void Close() 
        {
        }

        public virtual void Delete(string Vpath)
        {

        }
    
    }

    public class VirtualFile : VirtualDir
    {
        public string extention;
        public string DefultIconPath = "D:\\01 Kareem\\programing projects\\VirtualOS\\WpfApp1\\Assets\\icons\\Paomedia-Small-N-Flat-File-text.ico";
        public VirtualFile(string? _name, string? _virtualpath, string extention) :
            base(_name, _virtualpath)
        {
            this.extention = extention;
        }
    }

    public class VirtualIOFile : VirtualFile
    {
        public VirtualIOFile(string? _name, string? _virtualpath , string ext) :
            base(_name, _virtualpath , ext)
        { }
    }

    public class VirtualAppFile : VirtualFile 
    {
        public VirtualAppFile(string? _name, string? _virtualpath) :
            base(_name, _virtualpath , null)
        { }
    }

    public class TXTFile : VirtualFile
    {
        public TXTFile(string? _name, string? _virtualpath) :
            base(_name, _virtualpath , ".txt")
        { }
    }

    public class VirtualFolder : VirtualDir
    {
        public string? Name;
        public string DefultIconPath = "D:\\01 Kareem\\programing projects\\VirtualOS\\WpfApp1\\Assets\\icons\\directory-150354_960_720.webp";
        public List<VirtualDir> Children;
        public VirtualFolder(string? _name , string? _virtualpath) : 
            base(_name , _virtualpath) {
        }


    }



    //Core File System Engin
    public class VirtualDirController
    {
        public DeskDriver? driverTree;
        private List<VirtualDir>? CurrentPathNodes;
        private VirtualDir? currentUserNode;

        public void StartLoader()
        {
            DeskDriver desk = new DeskDriver("C:/", "C:/");
            driverTree = desk;
            driverTree.loadDeskTreeV2();
        }

        public void OpenExplorer(File_Explorer explorer)
        {

        }

    }
}
