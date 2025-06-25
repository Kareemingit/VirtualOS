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

    public static class AbstractFactory
    {
        public static TextBlock CreatePathBar(string currentPath)
        {
            return new TextBlock
            {
                Text = currentPath,
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(10)
            };
        }

        public static TreeView CreateSideTree(DeskDriver root)
        {
            TreeView tree = new TreeView();
            foreach (var child in root.Children)
            {
                if (child is VirtualFolder folder)
                {
                    var item = BuildTreeViewItem(folder);
                    tree.Items.Add(item);
                }
            }
            return tree;
        }

        private static TreeViewItem BuildTreeViewItem(VirtualFolder folder)
        {
            var item = new TreeViewItem { Header = folder.Name, Tag = folder };
            foreach (var child in folder.Children)
            {
                if (child is VirtualFolder subFolder)
                {
                    item.Items.Add(BuildTreeViewItem(subFolder));
                }
            }
            item.Selected += (s, e) =>
            {
                if (item.Tag is VirtualFolder selectedFolder && GetMainWindow() is MainWindow mainWindow)
                {
                    mainWindow.OpenExplorer(selectedFolder);
                }
                e.Handled = true;
            };
            return item;
        }

        public static ScrollViewer CreateIconGrid(List<VirtualDir> items)
        {
            var grid = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10) };

            foreach (var item in items)
            {
                grid.Children.Add(CreateIconForItem(item));
            }

            return new ScrollViewer
            {
                Content = grid,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }

        public static StackPanel CreateIconForItem(VirtualDir item)
        {
            string iconPath = item is VirtualFolder ?
                ((VirtualFolder)item).DefultIconPath :
                ((VirtualFile)item).DefultIconPath;

            var image = new Image
            {
                Width = 48,
                Height = 48,
                Source = new BitmapImage(new Uri(iconPath)),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var label = new TextBlock
            {
                Text = item.Name,
                Foreground = System.Windows.Media.Brushes.White,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var panel = new StackPanel
            {
                Width = 80,
                Height = 100,
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5),
                Tag = item,
                Background = System.Windows.Media.Brushes.Transparent
            };

            panel.Children.Add(image);
            panel.Children.Add(label);

            panel.MouseLeftButtonDown += (s, e) =>
            {
                if (item is VirtualFolder folder && GetMainWindow() is MainWindow mainWindow)
                {
                    mainWindow.OpenExplorer(folder);
                }
            };

            return panel;
        }

        private static MainWindow? GetMainWindow() => Application.Current.MainWindow as MainWindow;
    }


    public partial class File_Explorer : Window
    {
        private string fileIconPath = "D:/icons/file.png";
        private string folderIconPath = "D:/icons/folder.png";
        //VirtualDir initialDir, DeskDriver root
        public File_Explorer()
        {
            InitializeComponent();
            //BuildSidebar(root);
            //UpdateExplorerView(initialDir);
        }

        
        /*
                private void BuildSidebar(DeskDriver root)
                {
                    Sidebar.Items.Clear();

                    foreach (var item in root.Children)
                    {
                        if (item is VirtualFolder folder)
                        {
                            var node = CreateTreeItem(folder);
                            Sidebar.Items.Add(node);
                        }
                    }

                    Sidebar.SelectedItemChanged += (s, e) =>
                    {
                        if (Sidebar.SelectedItem is TreeViewItem treeItem && treeItem.Tag is VirtualDir dir)
                        {
                            UpdateExplorerView(dir);
                        }
                    };
                }

                private TreeViewItem CreateTreeItem(VirtualFolder folder)
                {
                    var item = new TreeViewItem { Header = folder.Name, Tag = folder };

                    foreach (var child in folder.Children)
                    {
                        if (child is VirtualFolder subFolder)
                            item.Items.Add(CreateTreeItem(subFolder));
                        else
                            item.Items.Add(new TreeViewItem { Header = child.Name, Tag = child });
                    }

                    return item;
                }

                private void UpdateExplorerView(VirtualDir dir)
                {
                    IconWrapPanel.Children.Clear();
                    PathBar.Text = dir.FullPath;

                    if (dir is VirtualFolder folder)
                    {
                        foreach (var child in folder.Children)
                        {
                            IconWrapPanel.Children.Add(CreateIcon(child));
                        }
                    }
                }

                private UIElement CreateIcon(VirtualDir item)
                {
                    string iconPath = item is VirtualFile ? fileIconPath : folderIconPath;

                    var stack = new StackPanel
                    {
                        Width = 80,
                        Height = 100,
                        Margin = new Thickness(10),
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    var image = new Image
                    {
                        Source = new BitmapImage(new Uri(iconPath, UriKind.Absolute)),
                        Width = 48,
                        Height = 48,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    var label = new TextBlock
                    {
                        Text = item.Name,
                        TextAlignment = TextAlignment.Center,
                        Foreground = System.Windows.Media.Brushes.Black,
                        FontSize = 12
                    };

                    stack.Children.Add(image);
                    stack.Children.Add(label);

                    return stack;
                }
        */
    }
}
