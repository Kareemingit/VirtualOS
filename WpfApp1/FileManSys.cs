using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace VirtualOS
{
    //struct Properties
    //{
    //    public int CPUusage;
    //    public double usedSpaceByts;
    //    public double usedSpaceUn;
    //    public double freeSpaceByts;
    //    public double freeSpaceUn;
    //    public int FilesCount;
    //    public int FoldersCount;
    //}

    public class DeskDriver : VirtualDir
    {
        private const string ActualPath = "D:/01 Kareem/programing projects/VirtualOS/WpfApp1/Root Desk";
        public List<VirtualDir> Children = new();

        public DeskDriver(string _name, string uipath) :
            base(_name, ActualPath, uipath)
        {
        }

        public string GetActualPath() { return ActualPath; }
        private List<VirtualDir> BuildTree(DirectoryInfo dir)
        {
            var list = new List<VirtualDir>();

            // Add directories
            foreach (var subDir in dir.GetDirectories())
            {
                var vFolder = new VirtualFolder(subDir.Name, subDir.FullName, "C:")
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
                    vFile = new TXTFile(file.Name, file.FullName, "tst");
                }
                else
                {
                    vFile = new VirtualFile(file.Name, file.FullName, file.Extension, "tst");
                }

                list.Add(vFile);
            }

            return list;
        }
        public void loadDeskTreeV1()
        {
            var rootInfo = new DirectoryInfo(ActualPath);
            foreach (var item in BuildTree(rootInfo))
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

            var rootFolder = new VirtualFolder(rootInfo.Name, rootInfo.FullName, "C:")
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
                    var subFolder = new VirtualFolder(subDir.Name, subDir.FullName, currentVirtualFolder.UIPath + "/" + subDir.Name)
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
                    string CurrUIPath = currentVirtualFolder.UIPath + "/" + file.Name;
                    if (file.Extension == ".txt")
                    {
                        vFile = new TXTFile(file.Name, file.FullName, CurrUIPath);
                    }
                    else
                    {
                        vFile = new VirtualFile(file.Name, file.FullName, file.Extension, CurrUIPath);
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
        public string? UIPath;
        public VirtualDir(string? name, string? virtualpath, string? uipath)
        {
            Name = name;
            Virtualpath = virtualpath;
            UIPath = uipath;
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
        public VirtualFile(string? _name, string? _virtualpath, string extention, string uipath) :
            base(_name, _virtualpath, uipath)
        {
            this.extention = extention;
        }
    }

    public class VirtualIOFile : VirtualFile
    {
        public VirtualIOFile(string? _name, string? _virtualpath, string ext, string uipath) :
            base(_name, _virtualpath, ext, uipath)
        { }
    }

    public class VirtualAppFile : VirtualFile
    {
        public VirtualAppFile(string? _name, string? _virtualpath, string uipath) :
            base(_name, _virtualpath, null, uipath)
        { }
    }

    public class TXTFile : VirtualFile
    {
        public TXTFile(string? _name, string? _virtualpath, string uipath) :
            base(_name, _virtualpath, ".txt", uipath)
        { }
    }

    public class VirtualFolder : VirtualDir
    {
        public List<VirtualDir> Children { get; set; } = new();
        public VirtualFolder(string? _name, string? _virtualpath, string? uipath) :
            base(_name, _virtualpath, uipath)
        {
        }

    }

    //Core File System Engin
    public class VirtualDirController
    {
        public DeskDriver? driverTree;
        private static VirtualDirController? _instance;
        public static VirtualDirController Instance => _instance ??= new VirtualDirController();

        public VirtualDirController() { }
        public void StartLoader()
        {
            if (driverTree != null)
                return;
            DeskDriver desk = new DeskDriver("C:", "C:");
            desk.Children = new List<VirtualDir>();
            desk.loadDeskTreeV2();
            driverTree = desk;
        }

        private void PlantFolderToHardWare(VirtualDir folder)
        {
            try
            {
                if (!Directory.Exists(folder.Virtualpath))
                {
                    Directory.CreateDirectory(folder.Virtualpath);
                }
                else
                {
                    MessageBox.Show($"Folder : {folder.UIPath} is Already Exist.");
                    return;
                }
            }
            catch (Exception ex){ 
                MessageBox.Show(ex.Message);
            }
        }
        private void PlantLaef(VirtualDir Newitem , VirtualDir distNode)
        {
            if(distNode is VirtualFolder f)
            {
                f.Children.Add(Newitem);
            }
            else if(distNode is DeskDriver desk)
            {
                desk.Children.Add(Newitem);
            }
        }
        public void PlantNewItem(VirtualDir Newitem , VirtualDir distNode)
        {
            if(Newitem is VirtualFolder folder)
            {
                PlantFolderToHardWare(folder);
            }
            else
            {

            }
            PlantLaef(Newitem , distNode);
        }

    }

    public class FileSysWacher
    {

    }

    public class VirtualLoader
    {

    }
}
