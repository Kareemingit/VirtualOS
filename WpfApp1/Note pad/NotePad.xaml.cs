using System;
using System.Collections.Generic;
using System.IO;
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

namespace VirtualOS.Note_pad
{
    /// <summary>
    /// Interaction logic for NotePad.xaml
    /// </summary>
    
    public partial class NotePad : Window
    {
        private VirtualFile file;
        private bool isFileSaved;
        public NotePad(VirtualFile _file)
        {
            InitializeComponent();
            isFileSaved = true;
            file = _file;
            Title = file.Name;
            Editor.Text = file.Read();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            file.Write(Editor.Text, file.Virtualpath);
            Title = file.Name;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (isFileSaved) return;
                Title = file.Name + " - Not Saved";
                
            }
            finally
            {
                isFileSaved = false;
            }

        }
    }
}
