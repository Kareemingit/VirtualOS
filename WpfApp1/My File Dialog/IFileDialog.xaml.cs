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
using System.IO;

namespace VirtualOS.My_File_Dialog
{
    
    public partial class IFileDialog : Window
    {
        DeskDriver RootDesk;
        public IFileDialog()
        {
            InitializeComponent();
            VirtualDirController virtualDirController = new VirtualDirController();
            virtualDirController.StartLoader();
            RootDesk = virtualDirController.driverTree;
            
            LoadDrives();
        }

        private void LoadDrives()
        {
            var item = CreateTreeItem(RootDesk ,RootDesk.Name , RootDesk.UIPath);
            item.Items.Add(null);
            FileTreeView.Items.Add(item);
        }

        private TreeViewItem CreateTreeItem(VirtualDir dir, string header, string fullPath)
        {
            var item = new TreeViewItem
            {
                Header = header,
                Tag = dir
            };
            if (dir is VirtualFolder || dir is DeskDriver)
            {
                item.Expanded += Folder_Expanded;
                if ((dir as VirtualFolder)?.Children?.Count > 0 || dir is DeskDriver)
                {
                    item.Items.Add(null);
                }
            }
            return item;
        }

        private void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;

            if (item.Items.Count == 1 && item.Items[0] == null)
            {
                item.Items.Clear();

                if (item.Tag is VirtualFolder folder)
                {
                    foreach (var dir in folder.Children)
                    {
                        var subItem = CreateTreeItem(dir, dir.Name, dir.UIPath);
                        if (dir is VirtualFolder || dir is DeskDriver)
                            subItem.Items.Add(null);
                        item.Items.Add(subItem);
                    }
                }
                else if (item.Tag is DeskDriver root)
                {
                    foreach (var dir in root.Children)
                    {
                        var subItem = CreateTreeItem(dir, dir.Name, dir.UIPath);
                        if (dir is VirtualFolder || dir is DeskDriver)
                            subItem.Items.Add(null);
                        item.Items.Add(subItem);
                    }
                }
            }
        }


        private void FileTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (FileTreeView.SelectedItem is TreeViewItem selectedItem)
            {
                if (selectedItem.Tag is VirtualFolder || selectedItem.Tag is DeskDriver)
                    return;
                VirtualDir file = selectedItem.Tag as VirtualFile;
                if (File.Exists(file.Virtualpath))
                {
                    MessageBox.Show($"You selected file:\n{file.UIPath}", "File Selected");
                }
            }
        }
    }
}
