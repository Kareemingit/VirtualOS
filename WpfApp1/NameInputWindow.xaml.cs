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
    /// Interaction logic for NameInputWindow.xaml
    /// </summary>
    
    public partial class NameInputWindow : Window
    {
        public string InputText { get; private set; } = "";

        public NameInputWindow()
        {
            InitializeComponent();
            NameTextBox.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            InputText = NameTextBox.Text.Trim();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}