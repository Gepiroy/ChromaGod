using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChromaGod {
    public class MyCanvas : Canvas{
        protected override void OnRender(DrawingContext dc) {
            if (App.bm == null) return;
            dc.DrawRectangle(Background, null, new System.Windows.Rect(0, 0, App.bm.Width, App.bm.Height));
            BitmapImage img = Global.BmToBmi(App.bm);
            dc.DrawImage(img, new System.Windows.Rect(0, 0, img.PixelWidth, img.PixelHeight));
        }
    }
}
