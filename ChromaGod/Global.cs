using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ChromaGod {
    public static class Global {
        public static BitmapImage BmToBmi(Bitmap bm) {
            BitmapImage bmi = new BitmapImage();
            using (MemoryStream memory = new MemoryStream()) {
                bm.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bmi.BeginInit();
                bmi.StreamSource = memory;
                bmi.CacheOption = BitmapCacheOption.OnLoad;
                bmi.EndInit();
            }
            return bmi;
        }
    }
}
