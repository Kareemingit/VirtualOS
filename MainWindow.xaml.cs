using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VirtualOS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public enum IconType { DEFAULT, FILE, FOLDER, APP }
    public class DesktopIcon : INotifyPropertyChanged
    {
        private double _x;
        private double _y;
        public string Name { get; set; }
        public string IconPath { get; set; }
        public IconType _IconType { get; set; }
        public double X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(); }
        }
        public double Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(); }
        }

        public IconType IconType
        {
            get => _IconType;
            set { _IconType = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string prop = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public partial class MainWindow : Window
    {
        public ObservableCollection<DesktopIcon> DIcons { get; set; } = new();
        private UIElement _draggedElement;
        private Point _dragOffset;
        private ImageSource DefaultWallpaper = new BitmapImage(new Uri("D:/01 Kareem/programing projects/VirtualOS/WpfApp1/Assets/wallpaper.png"));
        public MainWindow()
        {
            InitializeComponent();
            Wallpaper.Source = DefaultWallpaper;

            AddIcon(GenerateDeskTopIcon("Exeproer", IconType.DEFAULT));

        }

        private DesktopIcon GenerateDeskTopIcon(string name, IconType iconType)
        {
            DesktopIcon icon = new DesktopIcon();
            icon.Name = name;
            icon.IconType = iconType;
            if (icon.IconType == IconType.DEFAULT)
            {
                icon.IconPath = @"D:\01 Kareem\programing projects\VirtualOS\WpfApp1\Assets\icons\fileexp.png";
            }
            if (DIcons.Count == 0)
            {
                icon.X = 0;
                icon.Y = 20;
            }
            else if (DIcons.Count % 2 == 0)
            {
                icon.X = 0;
                icon.Y = DIcons[DIcons.Count - 1].Y + 90;
            }
            else
            {
                icon.X = DIcons[DIcons.Count - 1].X + 90;
                icon.Y = DIcons[DIcons.Count - 1].Y;
            }
            return icon;
        }

        private void AddIcon(DesktopIcon iconData)
        {
            DIcons.Add(iconData);
            var icon = new StackPanel
            {
                Width = 80,
                Height = 100,
                Orientation = Orientation.Vertical,
                DataContext = iconData
            };

            var image = new Image
            {
                Width = 48,
                Height = 48,
                Source = new BitmapImage(new Uri(iconData.IconPath)),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var label = new TextBlock
            {
                Text = iconData.Name,
                Foreground = System.Windows.Media.Brushes.White,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            icon.Children.Add(image);
            icon.Children.Add(label);

            //add context menu
            var contextMenu = new ContextMenu();
            var openItem = new MenuItem { Header = "Open", DataContext = iconData };
            openItem.Click += Open_Click;
            var deleteItem = new MenuItem { Header = "Delete", DataContext = iconData };
            deleteItem.Click += Delete_Click;
            contextMenu.Items.Add(openItem);
            contextMenu.Items.Add(deleteItem);
            icon.ContextMenu = contextMenu;

            // Add drag support
            icon.MouseLeftButtonDown += Icon_MouseLeftButtonDown;
            icon.MouseMove += Icon_MouseMove;
            icon.MouseLeftButtonUp += Icon_MouseLeftButtonUp;
            // Place on canvas
            Canvas.SetLeft(icon, iconData.X);
            Canvas.SetTop(icon, iconData.Y);
            DesktopCanvas.Children.Add(icon);
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is DesktopIcon icon)
            {
                MessageBox.Show($"Opening {icon.Name}...", "Info");
            }
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is DesktopIcon icon)
            {
                DIcons.Remove(icon);

                var toRemove = DesktopCanvas.Children
                    .OfType<StackPanel>()
                    .FirstOrDefault(sp => sp.DataContext == icon);
                if (toRemove != null)
                    DesktopCanvas.Children.Remove(toRemove);
            }
        }
        private void Icon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement icon)
            {
                _draggedElement = icon;
                _dragOffset = e.GetPosition(_draggedElement);
                icon.CaptureMouse();
            }
        }
        private void Icon_MouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedElement != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point pos = e.GetPosition(DesktopCanvas);
                Canvas.SetLeft(_draggedElement, pos.X - _dragOffset.X);
                Canvas.SetTop(_draggedElement, pos.Y - _dragOffset.Y);
            }
        }
        private void Icon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_draggedElement != null)
            {
                _draggedElement.ReleaseMouseCapture();
                _draggedElement = null;
            }
        }
        private void StartMenu_Click(object sender, RoutedEventArgs e)
        {
            var exitItem = new MenuItem { Header = "Shut Down" };
            exitItem.Click += (s, args) => Application.Current.Shutdown();

            var menu = new ContextMenu();
            menu.Items.Add(exitItem);

            // Attach the menu to the button
            if (sender is Button startButton)
            {
                menu.PlacementTarget = startButton;
                menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
                menu.IsOpen = true;
            }
        }
    }
}