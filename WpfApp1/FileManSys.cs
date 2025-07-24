using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
                    vFile = new UnSupportedFile(file.Name, file.FullName, file.Extension, "tst");
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
                        vFile.Load(vFile.Virtualpath);
                    }
                    else
                    {
                        vFile = new UnSupportedFile(file.Name, file.FullName, file.Extension, CurrUIPath);
                    }
                    currentVirtualFolder.Children.Add(vFile);
                }
            }

            // Attach root folder to DeskDriver
            Children = rootFolder.Children;
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

    }

    public abstract class VirtualFile : VirtualDir
    {
        public string extention;
        protected string Content;
        public VirtualFile(string? _name, string? _virtualpath, string extention, string uipath) :
            base(_name, _virtualpath, uipath)
        {
            this.extention = extention;
            this.Content = "";
        }
        public abstract string Read();
        public abstract void Write(string newContent , string distPath);
        public abstract void Load(string sourcePath);
    }

    public class TXTFile : VirtualFile
    {
        public TXTFile(string? _name , string? _virtualpath, string uipath) :
            base(_name, _virtualpath, ".txt", uipath)
        { }

        public override string Read()
        {
            return this.Content;
        }
        public override void Write(string newContent , string distPath)
        {
            this.Content = newContent;
            File.WriteAllText(distPath, this.Content);
        }
        public override void Load(string sourcePath)
        {
            string fileContent = File.ReadAllText(sourcePath);
            this.Content = fileContent;
        }
    }

    public class UnSupportedFile : VirtualFile
    {
        public UnSupportedFile(string? _name, string? _virtualpath, string ext, string uipath) :
            base(_name, _virtualpath, ext, uipath)
        { }

        public override string Read()
        {
            return null;
        }
        public override void Write(string newContent , string distPath)
        {

        }
        public override void Load(string sourcePath)
        {

        }
    }


    public class VirtualFolder : VirtualDir
    {
        public List<VirtualDir> Children { get; set; } = new();
        public VirtualFolder(string? _name, string? _virtualpath, string? uipath) :
            base(_name, _virtualpath, uipath)
        {
        }

    }

    class Pair
    {
        public bool isCopy { set; get; }
        public VirtualDir Dir { get; set; }
    }

    //Core File System Engin
    public class VirtualDirController
    {
        public DeskDriver? driverTree;
        private static VirtualDirController? _instance;
        public static VirtualDirController Instance => _instance ??= new VirtualDirController();
        private Pair userTempStorage;
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
        private void PlantFileToHardWare(VirtualDir file)
        {
            try
            {
                if (!File.Exists(file.Virtualpath))
                {
                    File.Create(file.Virtualpath);
                }
                else
                {
                    MessageBox.Show($"File : {file.UIPath} is Already Exist.");
                    return;
                }
            }
            catch (Exception ex)
            {
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
        private void uprootingFolderFromHardWare(VirtualFolder folder)
        {
            try
            {
                if (Directory.Exists(folder.Virtualpath))
                {
                    Directory.Delete(folder.Virtualpath, true);
                }
                else
                {
                    MessageBox.Show($"Folder : {folder.UIPath} is Already Exist.");
                    return;
                }
            }
            catch(IOException ioEx)
            {
                MessageBox.Show(ioEx.Message);
            }
        }
        private bool uprootingFolderFromTree(VirtualDir currentParent, VirtualDir itemToDelete)
        {
            if (currentParent == null) return false;
            List<VirtualDir>? childrenList = null;

            if (currentParent is VirtualFolder folder)
            {
                childrenList = folder.Children;
            }
            else if (currentParent is DeskDriver desk)
            {
                childrenList = desk.Children;
            }

            if (childrenList != null)
            {
                
                int initialCount = childrenList.Count;
                childrenList.RemoveAll(child => child == itemToDelete);
                if (childrenList.Count < initialCount)
                {
                    itemToDelete = null;
                    return true;
                }
                
                foreach (var child in childrenList)
                {
                    if (child is VirtualFolder || child is DeskDriver)
                    {
                        if (uprootingFolderFromTree(child, itemToDelete))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private void uprootingFileFromHardWare(VirtualFile file)
        {
            try
            {
                if(file is null) return;
                if (File.Exists(file.Virtualpath))
                {
                    File.Delete(file.Virtualpath);
                }
                else
                {
                    MessageBox.Show($"File : {file.Virtualpath} does not exist.");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void uprootingFileFromTree(VirtualDir currentParent, VirtualDir itemToDelete)
        {
            if(currentParent is null) return;
            List<VirtualDir>? childrenList = null;

            if (currentParent is VirtualFolder folder)
            {
                childrenList = folder.Children;
            }
            else if (currentParent is DeskDriver desk)
            {
                childrenList = desk.Children;
            }
            if (childrenList != null)
            {
                int currChildrenNumber = childrenList.Count;
                childrenList.RemoveAll(child => child == itemToDelete);
                if (childrenList.Count < currChildrenNumber)
                    return;
                foreach (var item in childrenList)
                {
                    if(item is VirtualFolder || item is DeskDriver){
                        uprootingFolderFromTree(item, itemToDelete);
                    }
                }
            }
        }
        private void SetFileCopyInstaceInTreeAndHardWare(VirtualDir item , VirtualDir DistnationDir)
        {
            string dpath = DistnationDir.Virtualpath + "\\" + item.Name;
            File.Copy(item.Virtualpath , dpath);
            if (item is TXTFile tXTFile)
            {
                TXTFile copyInstanc = new TXTFile(
                    item.Name,
                    DistnationDir.Virtualpath+ "\\" + tXTFile.Name,
                    DistnationDir.UIPath + "\\" + tXTFile.Name
                );
                copyInstanc.Write(tXTFile.Read() , DistnationDir.Virtualpath + "\\" + tXTFile.Name);
                PlantLaef(copyInstanc, DistnationDir);
            }
            else
            {
                UnSupportedFile unSupportedFile = new UnSupportedFile(
                    item.Name,
                    DistnationDir.Virtualpath + item.Name,
                    "",
                    DistnationDir.UIPath + item.Name
                );
                PlantLaef(unSupportedFile , DistnationDir);
            }
        }
        public void PlantNewItem(VirtualDir Newitem , VirtualDir distNode)
        {
            if(Newitem is VirtualFolder folder)
            {
                PlantFolderToHardWare(folder);
            }
            else if(Newitem is VirtualFile file)
            {
                PlantFileToHardWare(file);
            }
            PlantLaef(Newitem , distNode);
        }
        public void DeleteFolder(VirtualFolder folder)
        {
            uprootingFolderFromHardWare(folder);
            uprootingFolderFromTree(driverTree ,folder);
        }
        public void DeleteFile(VirtualFile file) 
        {
            uprootingFileFromHardWare(file);
            uprootingFileFromTree(driverTree,file);
        }
        public bool isThereAPair()
        {
            if (userTempStorage != null) return true;
            return false;
        }
        public void TakeCopyFromItem(VirtualDir dir)
        {
            Pair pair = new Pair { 
                Dir = dir,
                isCopy = true
            };
            userTempStorage = pair;
        }
        public void TakeCutFromItem(VirtualDir dir)
        {
            Pair pair = new Pair
            {
                Dir = dir,
                isCopy = false
            };
            userTempStorage = pair;
        }
        public void PasteItemInTempStorage(VirtualDir DistnationDir)
        {
            if(userTempStorage == null) return;
            VirtualDir item = userTempStorage.Dir;
            if(item is VirtualFolder)
            {

            }
            else
            {
                SetFileCopyInstaceInTreeAndHardWare(item, DistnationDir);
                if (!userTempStorage.isCopy)
                {
                    DeleteFile((VirtualFile)item);
                    userTempStorage = null;
                }
            }
            
        }
    }
}
