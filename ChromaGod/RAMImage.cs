using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChromaGod {
    public class RAMImage {
        public List<Color> palette = new List<Color>();
        public Color[] pixels;
        LockBitmap lsrc;
        public RAMImage(LockBitmap lb){
            lsrc = lb;
            int w = lb.Width, h = lb.Height, l = w * h, LinB = l * 4;
            pixels = new Color[l];
            byte r = 0, g = 0, b = 0;
            int bc = 0, index = 0;
            Color set = Color.Empty;
            foreach (byte by in lb.Pixels) {
                switch (bc++) {
                    case 0: b = by; break;
                    case 1: g = by; break;
                    case 2: r = by; break;
                    default: {
                            //ncol = Color.FromArgb(by,r,g,b);
                            //set = ncol;
                            foreach (Color c in palette) {
                                if (r==c.R&& g == c.G&& b == c.B) {
                                    set = c;
                                    break;
                                }
                            }
                            if (set.IsEmpty) {
                                set = Color.FromArgb(by, r, g, b);
                                palette.Add(set);
                            }
                            //pixels[index] = set;
                            set = Color.Empty;
                            index++;
                            bc = 0;
                            break;
                        }
                }
            }
        }
        public void applyToBitmap() {
            int w = lsrc.Width, h = lsrc.Height, l = w * h, LinB = l * 4;
            MemoryStream stream = new MemoryStream();
            foreach (Color c in pixels) {
                stream.WriteByte(c.B);
                stream.WriteByte(c.G);
                stream.WriteByte(c.R);
                stream.WriteByte(c.A);
            }
            stream.Flush();
            lsrc.Pixels = stream.ToArray();
        }
    }
}
