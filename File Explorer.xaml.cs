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

        public static void CreateSidebar(TreeView sidebar, List<VirtualDir> rootItems)
        {
            sidebar.Items.Clear();

            foreach (var item in rootItems)
            {
                if (item is VirtualDir vDir)
                {
                    var treeItem = CreateTreeViewItem(vDir);
                    sidebar.Items.Add(treeItem);
                }
            }
        }

        private static TreeViewItem CreateTreeViewItem(VirtualDir vDir)
        {
            var item = new TreeViewItem { Header = vDir.Name, Tag = vDir , Foreground = Brushes.Black };
            
            if (vDir is VirtualFolder folder)
            {
                foreach (var child in folder.Children)
                {
                    item.Items.Add(CreateTreeViewItem(child));
                }
            }
            return item;
        }
        public static void CreateIconViewer(WrapPanel iconWrapPanel, List<VirtualDir> items, string fileIconPath, string folderIconPath)
        {
            iconWrapPanel.Children.Clear();
            if (items == null) return;
            foreach (var item in items)
            {
                string iconPath = (item is VirtualFolder) ? folderIconPath : fileIconPath;
                iconWrapPanel.Children.Add(CreateIcon(item, iconPath));
            }
        }

        private static StackPanel CreateIcon(VirtualDir item, string iconPath)
        {
            var icon = new StackPanel
            {
                Width = 80,
                Height = 100,
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5)
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
        //private string fileIconPath = "D:/icons/file.png";
        //private string folderIconPath = "D:/icons/folder.png";
        //VirtualDir initialDir, DeskDriver root
        public File_Explorer(VirtualDir currentDir, List<VirtualDir> rootStructure)
        {
            InitializeComponent();

            string fileIconPath = "D:/01 Kareem/programing projects/VirtualOS/WpfApp1/Assets/icons/Paomedia-Small-N-Flat-File-text.ico";
            string folderIconPath = "D:/01 Kareem/programing projects/VirtualOS/WpfApp1/Assets/icons/directory-150354_960_720.webp";

            PathBar.Text = "Current Path: ";
            ExplorerUIFactory.CreatePathBar(PathBar, currentDir.Virtualpath);
            ExplorerUIFactory.CreateSidebar(Sidebar, rootStructure);

            if (currentDir is VirtualFolder folder)
            {
                ExplorerUIFactory.CreateIconViewer(IconWrapPanel, folder.Children, fileIconPath, folderIconPath);
            }
        }
    }
}
