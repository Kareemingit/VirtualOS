using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VirtualOS.Note_pad;

namespace VirtualOS
{
    /// <summary>
    /// Interaction logic for File_Explorer.xaml
    /// </summary>

    public static class ExplorerUIFactory
    {
        public static void CreatePathBar(TextBlock pathBar, string currentPath)
        {
            pathBar.Text += $"{currentPath}";
        }
        public static void CreateSidebar(TreeView sidebar, List<VirtualDir> rootItems, File_Explorer parentWindow)
        {
            sidebar.Items.Clear();

            foreach (var item in rootItems)
            {
                if (item is VirtualDir vDir)
                {
                    var treeItem = CreateTreeViewItem(vDir, parentWindow);
                    sidebar.Items.Add(treeItem);
                }
            }
        }
        private static TreeViewItem CreateTreeViewItem(VirtualDir vDir, File_Explorer parentWindow)
        {
            var item = new TreeViewItem
            {
                Header = vDir.Name,
                Tag = vDir,
                Foreground = Brushes.Black,
                ContextMenu = CreateContextMenuFor(vDir, parentWindow)
            };

            if (vDir is VirtualFolder folder)
            {
                foreach (var child in folder.Children)
                {
                    item.Items.Add(CreateTreeViewItem(child, parentWindow));
                }
            }
            return item;
        }
        private static ContextMenu CreateContextMenuFor(VirtualDir vDir, File_Explorer parentWindow)
        {
            var menu = new ContextMenu();

            // "Open" is common for both
            var openItem = new MenuItem { Header = "Open", Tag = vDir };
            openItem.Click += (s, e) =>
            {
                parentWindow.Open(vDir);
            };

            menu.Items.Add(openItem);

            if (vDir is VirtualFolder folder)
            {
                // "New >" submenu
                var newSubMenu = new MenuItem { Header = "New .." };

                var newFolderItem = new MenuItem { Header = "Folder", Tag = vDir };
                newFolderItem.Click += (s, e) =>
                {
                    parentWindow.CreateNewFolder(vDir);
                };
                var newFileItem = new MenuItem { Header = "File Document", Tag = vDir };

                newFileItem.Click += (s, e) =>
                {
                    parentWindow.CreateNewFile(vDir);
                };

                newSubMenu.Items.Add(newFolderItem);
                newSubMenu.Items.Add(newFileItem);

                menu.Items.Add(newSubMenu);
            }

            // "Delete" is common for both
            var deleteItem = new MenuItem { Header = "Delete", Tag = vDir };
            deleteItem.Click += (s, e) =>
            {
                if (s is MenuItem item && item.Tag is VirtualDir deletedItem)
                {
                    parentWindow.DeleteNode(deletedItem);
                }
            };

            var renameItem = new MenuItem { Header = "Rename .. ", Tag = vDir };
            renameItem.Click += (s, e) =>
            {
                parentWindow.RenameItem(vDir);
            };

            var CopyItem = new MenuItem { Header = "Copy", Tag = vDir };
            CopyItem.Click += (s, e) =>
            {
                parentWindow.CopyItem(vDir);
            };

            var CutItem = new MenuItem { Header = "Cut", Tag = vDir };
            CutItem.Click += (s, e) =>
            {
                parentWindow.CutItem(vDir);
            };

            menu.Items.Add(deleteItem);
            menu.Items.Add(renameItem);
            menu.Items.Add(CopyItem);
            menu.Items.Add(CutItem);
            return menu;
        }
        public static void CreateIconViewer(WrapPanel iconWrapPanel, List<VirtualDir> items, string fileIconPath, string folderIconPath,string unknownFileTypeIconPath, File_Explorer parentWindow)
        {
            iconWrapPanel.Children.Clear();
            if (items == null) return;
            foreach (var item in items)
            {
                string iconPath = null;
                if (item is VirtualFolder vDir)
                {
                    iconPath = folderIconPath;
                }
                else if (item is TXTFile vFile) {
                    iconPath = fileIconPath;
                }
                else
                {
                    iconPath = unknownFileTypeIconPath;
                }
                iconWrapPanel.Children.Add(CreateIcon(item, iconPath, parentWindow));
            }
        }
        private static StackPanel CreateIcon(VirtualDir item, string iconPath, File_Explorer parentWindow)
        {
            var icon = new StackPanel
            {
                Width = 80,
                Height = 100,
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5),
                ContextMenu = CreateContextMenuFor(item, parentWindow),
                DataContext = item
            };

            var image = new Image
            {
                Width = 48,
                Height = 48,
                Source = new BitmapImage(new Uri(iconPath, UriKind.Absolute)),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var label = new TextBlock
            {
                Text = item.Name,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            icon.Children.Add(image);
            icon.Children.Add(label);

            return icon;
        }
    }

    public partial class File_Explorer : Window
    {
        FileManager core = new FileManager();
        DeskDriver Root;
        string fileIconPath = "D:/01 Kareem/programing projects/VirtualOS/WpfApp1/Assets/icons/Paomedia-Small-N-Flat-File-text.ico";
        string folderIconPath = "D:/01 Kareem/programing projects/VirtualOS/WpfApp1/Assets/icons/directory-150354_960_720.webp";
        string unknownFileTypeIconPath = "D:\\01 Kareem\\programing projects\\VirtualOS\\WpfApp1\\Assets\\icons\\9166568.png";
        private VirtualDir currentDir;
        public VirtualDir CurrentDir => currentDir;
        public File_Explorer(DeskDriver root, List<VirtualDir> rootStructure)
        {
            InitializeComponent();
            Root = root;
            PathBar.Text = $"Current Path: ";
            ExplorerUIFactory.CreatePathBar(PathBar, root.UIPath);
            ExplorerUIFactory.CreateSidebar(Sidebar, rootStructure, this);
            core.RunRoot();
            currentDir = root;
            if (root is DeskDriver desk)
            {
                ExplorerUIFactory.CreateIconViewer(IconWrapPanel, root.Children, fileIconPath, folderIconPath ,unknownFileTypeIconPath, this);
            }
        }

        private string TakeInput()
        {
            string output = null;
            var inputWindow = new NameInputWindow { Owner =this};
            if (inputWindow.ShowDialog() == true && !string.IsNullOrEmpty(inputWindow.InputText))
            {
                output = inputWindow.InputText;
            }
            return output;
        }
        public void RefreshSidebarAndIcons()
        {
            ExplorerUIFactory.CreateSidebar(Sidebar, Root.Children, this);

            if (currentDir is VirtualFolder folder)
            {
                ExplorerUIFactory.CreateIconViewer(IconWrapPanel, folder.Children, fileIconPath, folderIconPath , unknownFileTypeIconPath, this);
            }
            else if(currentDir is DeskDriver root)
            {
                ExplorerUIFactory.CreateIconViewer(IconWrapPanel, root.Children, fileIconPath, folderIconPath , unknownFileTypeIconPath, this);
            }
        }
        public void UpdatePathAndIcons(VirtualDir vDir)
        {
            PathBar.Text = "Current Path: " + vDir.UIPath;

            if (vDir is VirtualFolder folder)
            {
                ExplorerUIFactory.CreateIconViewer(IconWrapPanel, folder.Children, fileIconPath, folderIconPath ,unknownFileTypeIconPath, this);
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            VirtualDir bDir = core.BackTrack();
            if (bDir == null) { return; }
            else if (bDir is DeskDriver desk)
            {
                ExplorerUIFactory.CreateIconViewer(IconWrapPanel, desk.Children, fileIconPath, folderIconPath, unknownFileTypeIconPath, this);
                PathBar.Text = $"Current Path: {desk.Name}";
            }
            else
            {
                UpdatePathAndIcons(bDir);
            }
            currentDir = bDir;
        }
        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            VirtualDir fDir = core.GoForward();
            if (fDir == null) { return; }
            else if (fDir is DeskDriver desk)
            {
                ExplorerUIFactory.CreateIconViewer(IconWrapPanel, desk.Children, fileIconPath, folderIconPath , unknownFileTypeIconPath, this);
                PathBar.Text = $"Current Path: {desk.Name}";
            }
            else
            {
                UpdatePathAndIcons(fDir);
            }
            currentDir = fDir;
        }
        public void Open(VirtualDir vDir)
        {
            core.OpenNew(vDir);

            if (vDir is TXTFile txtFile)
            {
                var textEditor = new NotePad(txtFile);
                textEditor.Show();
            }
            else if (vDir is VirtualFolder folder){
                currentDir = vDir;
                UpdatePathAndIcons(folder);
            }
        }
        private void RemoveIcon(VirtualDir target) 
        {
            if (currentDir == null)
            {
                var icon = IconWrapPanel.Children
                    .OfType<StackPanel>()
                    .FirstOrDefault(sp => sp.DataContext == target);

                if (icon != null)
                {
                    IconWrapPanel.Children.Remove(icon);
                }
                return;
            }

            else
            {
                // We're inside a folder → remove icon from icon viewer (UI only)
                var icon = IconWrapPanel.Children
                    .OfType<StackPanel>()
                    .FirstOrDefault(sp => sp.DataContext == target);

                if (icon != null)
                {
                    IconWrapPanel.Children.Remove(icon);
                }
            }
            // Update UI
        }
        public void DeleteNode(VirtualDir target)
        {
            core.DeleteNode(target);
            RemoveIcon(target);
            RefreshSidebarAndIcons();
        }
        private void MainScrollViewer_RightClick(object sender, MouseButtonEventArgs e)
        {
            var result = VisualTreeHelper.HitTest(IconWrapPanel, e.GetPosition(IconWrapPanel));
            if (result?.VisualHit is FrameworkElement element && element.DataContext is VirtualDir)
                return;

            var contextMenu = new ContextMenu();

            var newSubMenu = new MenuItem { Header = "New .." };

            var newFolderItem = new MenuItem { Header = "Folder" };
            newFolderItem.Click += (s, args) =>
            {
                CreateNewFolder(currentDir);
            };

            var newFileItem = new MenuItem { Header = "File Document" };
            newFileItem.Click += (s, args) =>
            {
                CreateNewFile(currentDir);
            };

            var pasteSubMenu = new MenuItem { Header = "Past" };
            pasteSubMenu.Click += (s, args) =>
            {
                paste();
            };
            if(!core.isThereApair())
                pasteSubMenu.IsEnabled = false;
            
            newSubMenu.Items.Add(newFolderItem);
            newSubMenu.Items.Add(newFileItem);
            contextMenu.Items.Add(pasteSubMenu);
            contextMenu.Items.Add(newSubMenu);

            contextMenu.Placement = PlacementMode.MousePoint;
            contextMenu.IsOpen = true;
        }
        private void paste()
        {
            core.PasteItem(currentDir);
            RefreshSidebarAndIcons();
        }
        public void CreateNewFolder(VirtualDir distinationNode)
        {
            string takeNameInput = TakeInput();
            if (takeNameInput != null)
            {
                core.CreateNewFolder(takeNameInput, distinationNode);
                RefreshSidebarAndIcons();
            }
        }
        public void CreateNewFile(VirtualDir distinationNode)
        {
            string takeNameInput = TakeInput();
            if (takeNameInput != null)
            {
                core.CreateNewFile(takeNameInput , distinationNode);
                RefreshSidebarAndIcons();
            }
        }
        public void RenameItem(VirtualDir virtualDir)
        {
            string NewName = TakeInput();
            if (NewName != null)
            {
                core.RenameItem(virtualDir, NewName);
                RefreshSidebarAndIcons();
            }
        }
        public void CopyItem(VirtualDir virtualDir)
        {
            core.CopyItem(virtualDir);
        }
        public void CutItem(VirtualDir virtualDir)
        {
            core.CutItem(virtualDir);
        }
    }
}