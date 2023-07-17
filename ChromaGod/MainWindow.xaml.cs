using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace ChromaGod {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public static MainWindow instance;
        public MainWindow() {
            InitializeComponent();
            updateFilters();
            instance = this;
        }
        void updateFilters() {
            int r = 0, g = 0, b = 0;
            foreach (ColP c in App.filters) {
                if(c.Parent!=colorList)colorList.Children.Add(c);
                r += c.color.R;
                g += c.color.G;
                b += c.color.B;
            }
            r /= App.filters.Count();
            g /= App.filters.Count();
            b /= App.filters.Count();
            r = 255 - r;
            g = 255 - g;
            b = 255 - b;
            canv.Background = new SolidColorBrush(Color.FromRgb((byte)r, (byte)g, (byte)b));
            update();
        }
        public void update() {
            canv.InvalidateVisual();
        }

        private void canvMouseUp(object sender, MouseButtonEventArgs e) {
            Point p = e.GetPosition(canv);
            p.X *= 2;
            p.Y *= 2;
            System.Drawing.Color c = App.source.GetPixel((int)p.X, (int)p.Y);
            if (e.ChangedButton==MouseButton.Right) {
                App.filters.Add(new ColP(Color.FromRgb(c.R, c.G, c.B), 20));
            } else {
                App.selColP.setColor(Color.FromRgb(c.R, c.G, c.B));
            }
            updateFilters();
            App.filter();
            update();
        }

        private void btnRender(object sender, RoutedEventArgs e) {
            App.render();
        }
    }
}
