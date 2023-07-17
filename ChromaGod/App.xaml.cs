using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ChromaGod {
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application {
        public static string folderpath;
        public static Bitmap source, bm;
        public static List<ColP> filters = new List<ColP>();
        public static ColP selColP;
        protected override void OnStartup(StartupEventArgs e) {
            folderpath = AppDomain.CurrentDomain.BaseDirectory + "cg_data\\";
            string firstFileName = Directory.GetFiles(folderpath + "input\\")[0];
            loadImage(firstFileName);
            filters.Add(new ColP(System.Windows.Media.Color.FromRgb(255,255,255),20));
            //filters.Add(new ColP(System.Windows.Media.Color.FromRgb(0, 0, 0), 5));
            //filters.Add(new ColP(System.Windows.Media.Color.FromRgb(128, 100, 35), 75));
            selColP = filters[0];
            selColP.update();
            filter();
        }
        void loadImage(string st) {
            source = new Bitmap(st);
            bm = new Bitmap(source);
        }
        static Color empty = Color.FromArgb(0,0,0,0);
        public static void filter() {
            test();
            /*long timer = DateTime.Now.Ticks;
            bm = new Bitmap(source);
            bool[,] mask = boldMask(boolMap(bm));
            applyMask(bm, mask);
            //ProcessUsingLockbits(bm);
            Console.WriteLine("ms to filter: " + (DateTime.Now.Ticks - timer) / TimeSpan.TicksPerMillisecond);
            bm = new Bitmap(bm, source.Width / 2, source.Height / 2);
            timer = DateTime.Now.Ticks - timer;
            Console.WriteLine("ms to all: "+timer/TimeSpan.TicksPerMillisecond);*/
        }

        public static void render() {
            string outDir = "output\\render" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            Directory.CreateDirectory(folderpath+outDir+"\\");
            bool b = true;
            Thread t = new Thread(() => {
                foreach (string st in Directory.GetFiles(folderpath + "input\\")) {
                    string outp = st.Replace("input", outDir);
                    bm = new Bitmap(st);
                    bool[,] mask = boldMask(boolMap(bm));
                    applyMask(bm, mask);
                    bm.Save(outp, ImageFormat.Png);
                    bm = new Bitmap(bm, source.Width / 2, source.Height / 2);
                    ChromaGod.MainWindow.instance.Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(() => {
                        ChromaGod.MainWindow.instance.update();
                    })).Wait();
                }
                b = false;
            });
            t.Start();
            /*while (b) {
                Thread.Sleep(500);
                ChromaGod.MainWindow.instance.update();
            }*/
            /*foreach (string st in Directory.GetFiles(folderpath + "input\\")) {
                string outp = st.Replace("input", outDir);
                bm = new Bitmap(st);
                bool[,] mask = boldMask(boolMap(bm));
                applyMask(bm, mask);
                bm.Save(outp, ImageFormat.Png);
                bm = new Bitmap(bm, source.Width / 2, source.Height / 2);
                ChromaGod.MainWindow.instance.update();
            }*/
        }
        static bool near(int i, int c, int r) {
            return i >= c-r && i <= c+r;
        }

        static void test() {
            long timer = DateTime.Now.Ticks;
            bm = new Bitmap(source);
            tlog(ref timer, "to open bm from source: $Tms");
            LockBitmap lb = new LockBitmap(bm);
            tlog(ref timer, "to create lbm: $Tms");
            lb.LockBits();
            tlog(ref timer, "to lock bits: $Tms");
            bool[,] mask = boolMap2(lb);
            tlog(ref timer, "to build mask: $Tms");
            mask = boldMask(mask);
            tlog(ref timer, "to bold mask: $Tms");
            applyMask2(lb, mask);
            tlog(ref timer, "to apply mask: $Tms");
            lb.UnlockBits();
            tlog(ref timer, "to unlock bits: $Tms");
            bm = new Bitmap(bm, source.Width / 2, source.Height / 2);
            tlog(ref timer, "to scaledown: $Tms");
        }

        static void tlog(ref long timer, string mes) {
            Console.WriteLine(mes.Replace("$T", ""+(DateTime.Now.Ticks - timer) / TimeSpan.TicksPerMillisecond));
            timer = DateTime.Now.Ticks;
        }

        private static void applyMask(Bitmap processedBitmap, bool[,] mask) {
            BitmapData bitmapData = processedBitmap.LockBits(new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height), ImageLockMode.ReadWrite, processedBitmap.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(processedBitmap.PixelFormat) / 8;
            int byteCount = bitmapData.Stride * processedBitmap.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;

            for (int y = 0; y < heightInPixels; y++) {
                int currentLine = y * bitmapData.Stride;
                for (int x = 0; x < widthInBytes; x += bytesPerPixel) {
                    if (!mask[x/bytesPerPixel,y])pixels[currentLine + x + 3] = 0;//set alpha to 0
                }
            }
            // copy modified bytes back
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            processedBitmap.UnlockBits(bitmapData);
        }
        private static bool[,] boolMap(Bitmap pb) {
            bool[,] ret = new bool[pb.Width, pb.Height];

            BitmapData bitmapData = pb.LockBits(new Rectangle(0, 0, pb.Width, pb.Height), ImageLockMode.ReadWrite, pb.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(pb.PixelFormat) / 8;
            int byteCount = bitmapData.Stride * pb.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;
            Console.WriteLine("ret x=" + ret.GetLength(0) + "; y=" + ret.GetLength(1));
            Console.WriteLine("bm x=" + bm.Width + "; y=" + bm.Height);
            for (int y = 0; y < heightInPixels; y++) {
                int currentLine = y * bitmapData.Stride;
                for (int x = 0; x < widthInBytes; x += bytesPerPixel) {
                    byte b = pixels[currentLine + x];
                    byte g = pixels[currentLine + x + 1];
                    byte r = pixels[currentLine + x + 2];
                    byte a = pixels[currentLine + x + 3];
                    bool destroy = true;
                    foreach (ColP f in filters) {
                        if (near(r, f.color.R, f.range) && near(g, f.color.G, f.range) && near(b, f.color.B, f.range)) {
                            destroy = false;
                            break;
                        }
                    }
                    //Console.WriteLine("x="+x+"; y="+y);
                    ret[x/bytesPerPixel, y] = !destroy;
                }
            }
            pb.UnlockBits(bitmapData);

            return ret;
        }
        private static bool[,] boolMap2(LockBitmap lb) {
            int w= lb.Width, h=lb.Height, l=w*h, LinB=l*4;
            bool[,] ret = new bool[w, h];
            byte r=0, g=0, b=0;
            bool save;
            int bc = 0;
            int index = 0;
            
            foreach(byte by in lb.Pixels) {
                switch (bc++) {
                    case 0: b = by; break;
                    case 1: g = by; break;
                    case 2: r = by; break;
                    default: {
                            save = false;
                            foreach (ColP f in filters) {
                                if (near(r, f.color.R, f.range) && near(g, f.color.G, f.range) && near(b, f.color.B, f.range)) {
                                    save = true;
                                    break;
                                }
                            }
                            ret[index % w, index / w] = save;
                            index++;
                            bc = 0;
                            break;
                    }
                }
            }
            /*for (int s=0;s<LinB;s+=4) {//Очень быстро
                b = lb.Pixels[s];
                g = lb.Pixels[s+1];
                r = lb.Pixels[s+2];
                save = false;
                foreach (ColP f in filters) {
                    if (near(r, f.color.R, f.range) && near(g, f.color.G, f.range) && near(b, f.color.B, f.range)) {
                        save = true;
                        break;
                    }
                }
                //preRet[index++] = !destroy;//Слишком дохрена оптимизации.
                ret[index % w, index / w]=save;
                index++;
            }*/
            return ret;
        }
        private static void applyMask2(LockBitmap lb, bool[,] mask) {
            int w = lb.Width, h = lb.Height, l = w * h, LinB = l * 4;
            int index = 0;
            for (int s = 0; s < LinB; s += 4) {//Очень быстро
                if (!mask[index % w, index / w]) lb.Pixels[s + 3] = 0;
                index++;
            }
        }
        private static int boldy = 3, killoners=5; //(1 of killoners is self.)
        static bool[,] boldMask(bool[,] bools) {
            bool[,] ret = new bool[bools.GetLength(0), bools.GetLength(1)];
            for (int x=0;x<bools.GetLength(0);x ++) {
                for (int y = 0; y < bools.GetLength(1); y++) {
                    if (bools[x, y]) {
                        int ns = 0;
                        for (int dx = -1; dx <= 1; dx++) {//search for aloners and KILL THEM ALL!!!
                            for (int dy = -1; dy <= 1; dy++) {
                                if (getbm(ref bools, x + dx, y + dy)) ns++;
                            }
                        }
                        if (ns <= killoners) continue;//KILL KILL KILL KIIIL!!!! (1 of them is self.)
                        for (int dx=-boldy;dx<=boldy;dx++) {
                            for (int dy = -boldy; dy <= boldy; dy++) {
                                setbm(ref ret, x+dx, y+dy, true);
                            }
                        }
                        
                    }
                }
            }
            return ret;
        }
        static bool getbm(ref bool[,] mask, int x, int y) {
            if (x < 0 || x >= mask.GetLength(0)) return false;
            if (y < 0 || y >= mask.GetLength(1)) return false;
            return mask[x, y];
        }
        static void setbm(ref bool[,] mask, int x, int y, bool b) {
            if (x < 0 || x >= mask.GetLength(0))return;
            if (y < 0 || y >= mask.GetLength(1)) return;
            mask[x, y] = b;
        }
    }
}
