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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChromaGod {
    /// <summary>
    /// Логика взаимодействия для ColP.xaml
    /// </summary>
    public partial class ColP : UserControl {
        public Color color;
        public int range;
        ColP() {
            InitializeComponent();
        }
        public ColP(Color color, int range) {
            InitializeComponent();
            setColor(color);
            setRange(range);
        }
        public void setColor(Color color) {
            this.color = color;
            colDispl.Fill = new SolidColorBrush(color);
        }
        public void setRange(int range) {
            this.range = range;
            rangeDispl.Text = range.ToString();
        }
        public void update() {
            colBorder.BorderBrush = App.selColP == this ? Brushes.Yellow : Brushes.Black;
        }
        private void colMup(object sender, MouseButtonEventArgs e) {
            App.selColP.colBorder.BorderBrush = Brushes.Black;
            App.selColP = this;
            colBorder.BorderBrush = Brushes.Yellow;
        }

        private void textChanged(object sender, TextChangedEventArgs e) {
            /*if (rangeDispl.IsKeyboardFocused) {
                if(int.TryParse(rangeDispl.Text, out range)) {
                    App.filter();
                    MainWindow.instance.update();
                }
            }*/
        }

        private void textKeyUp(object sender, KeyEventArgs e) {
            if (e.Key==Key.Enter&&int.TryParse(rangeDispl.Text, out range)) {
                App.filter();
                MainWindow.instance.update();
            }
        }
    }
}
