using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VirtualOS.Invisable_ink;
using VirtualOS.My_File_Dialog;

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
        private FileManager fileManager = new FileManager();
        public File_Explorer explorer = null;

        public ObservableCollection<DesktopIcon> DIcons { get; set; } = new();
        private UIElement _draggedElement;
        private Point _dragOffset;
        private ImageSource DefaultWallpaper = new BitmapImage(new Uri("D:/01 Kareem/programing projects/VirtualOS/WpfApp1/Assets/wallpaper.png"));
        public MainWindow()
        {
            InitializeComponent();
            Wallpaper.Source = DefaultWallpaper;
            fileManager.Load();
            AddIcon(GenerateDeskTopIcon("Exeproer", IconType.DEFAULT));
            AddIcon(GenerateDeskTopIcon("Invisable Ink", IconType.APP));
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
            if(icon.IconType == IconType.APP)
            {
                icon.IconPath = @"D:\01 Kareem\programing projects\VirtualOS\WpfApp1\Assets\icons\ChatAppIcon.png";
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
                DataContext = iconData,
                Background = Brushes.Transparent,
                IsHitTestVisible = true
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
            Panel.SetZIndex(icon, 99);

            DesktopCanvas.Children.Add(icon);
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is DesktopIcon icon)
            {
                if (icon.Name == "Exeproer")
                {
                    var root = fileManager.GetRoot();
                    if (root != null)
                    {
                        explorer = new File_Explorer(root, root.Children);
                        explorer.Show();
                        //IFileDialog dialog = new IFileDialog();
                        //dialog.Show();
                    }
                }
                else if (icon.Name == "Invisable Ink")
                {
                    var chatWindow = new InvisableInk();
                    chatWindow.Show();
                }
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
        private void DesktopCanvas_RightClick(object sender, MouseButtonEventArgs e)
        {
            // Hit test: check if click landed on an icon
            var result = VisualTreeHelper.HitTest(DesktopCanvas, e.GetPosition(DesktopCanvas));
            if (result?.VisualHit is FrameworkElement element && element.DataContext is DesktopIcon) return;


            var wallpaperItem = new MenuItem { Header = "Change Wallpaper" };
            wallpaperItem.Click += ChangeWallpaper_Click;

            var menu = new ContextMenu();
            menu.Items.Add(wallpaperItem);

            // Place menu at cursor position
            menu.Placement = PlacementMode.MousePoint;
            menu.IsOpen = true;
        }
        private void ChangeWallpaper_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Choose Wallpaper",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (dialog.ShowDialog() == true)
            {
                Wallpaper.Source = new BitmapImage(new Uri(dialog.FileName, UriKind.Absolute));
            }
        }
    }
}