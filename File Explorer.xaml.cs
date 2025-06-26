using System;
using System.Collections.Generic;
using System.Linq;
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
        public static void CreateSidebar(TreeView sidebar, List<VirtualDir> rootItems , File_Explorer parentWindow)
        {
            sidebar.Items.Clear();

            foreach (var item in rootItems)
            {
                if (item is VirtualDir vDir)
                {
                    var treeItem = CreateTreeViewItem(vDir , parentWindow);
                    sidebar.Items.Add(treeItem);
                }
            }
        }
        private static TreeViewItem CreateTreeViewItem(VirtualDir vDir , File_Explorer parentWindow)
        {
            var item = new TreeViewItem { 
                Header = vDir.Name, 
                Tag = vDir , 
                Foreground = Brushes.Black,
                ContextMenu = CreateContextMenuFor(vDir , parentWindow)
            };
            
            if (vDir is VirtualFolder folder)
            {
                foreach (var child in folder.Children)
                {
                    item.Items.Add(CreateTreeViewItem(child , parentWindow));
                }
            }
            return item;
        }

        private static ContextMenu CreateContextMenuFor(VirtualDir vDir , File_Explorer parentWindow)
        {
            var menu = new ContextMenu();

            // "Open" is common for both
            var openItem = new MenuItem { Header = "Open", Tag = vDir };
            openItem.Click += (s, e) => 
            MessageBox.Show($"Open: {vDir.Name}");

            menu.Items.Add(openItem);

            if (vDir is VirtualFolder folder)
            {
                // "New >" submenu
                var newSubMenu = new MenuItem { Header = "New .." };

                var newFolderItem = new MenuItem { Header = "Folder", Tag = vDir };
                newFolderItem.Click += (s, e) => MessageBox.Show($"New Folder inside: {vDir.Name}");

                var newFileItem = new MenuItem { Header = "File Document", Tag = vDir };
                newFileItem.Click += (s, e) => MessageBox.Show($"New File inside: {vDir.Name}");

                newSubMenu.Items.Add(newFolderItem);
                newSubMenu.Items.Add(newFileItem);

                menu.Items.Add(newSubMenu);
            }

            // "Delete" is common for both
            var deleteItem = new MenuItem { Header = "Delete", Tag = vDir };
            deleteItem.Click += (s, e) => MessageBox.Show($"Delete: {vDir.Name}");

            menu.Items.Add(deleteItem);

            return menu;
        }

        public static void CreateIconViewer(WrapPanel iconWrapPanel, List<VirtualDir> items, string fileIconPath, string folderIconPath , File_Explorer parentWindow)
        {
            iconWrapPanel.Children.Clear();
            if (items == null) return;
            foreach (var item in items)
            {
                string iconPath = (item is VirtualFolder) ? folderIconPath : fileIconPath;
                iconWrapPanel.Children.Add(CreateIcon(item, iconPath , parentWindow));
            }
        }
        private static StackPanel CreateIcon(VirtualDir item, string iconPath , File_Explorer parentWindow)
        {
            var icon = new StackPanel
            {
                Width = 80,
                Height = 100,
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5),
                ContextMenu = CreateContextMenuFor(item , parentWindow)
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
        public File_Explorer(DeskDriver root, List<VirtualDir> rootStructure)
        {
            InitializeComponent();

            string fileIconPath = "D:/01 Kareem/programing projects/VirtualOS/WpfApp1/Assets/icons/Paomedia-Small-N-Flat-File-text.ico";
            string folderIconPath = "D:/01 Kareem/programing projects/VirtualOS/WpfApp1/Assets/icons/directory-150354_960_720.webp";

            PathBar.Text = "Current Path: ";
            ExplorerUIFactory.CreatePathBar(PathBar, root.Path);
            ExplorerUIFactory.CreateSidebar(Sidebar, rootStructure , this);

            if (root is DeskDriver desk)
            {
                ExplorerUIFactory.CreateIconViewer(IconWrapPanel, root.Children, fileIconPath, folderIconPath , this);
            }
        }
    }
}