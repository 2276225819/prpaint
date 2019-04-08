using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
//using System.Windows.Controls;
//using System.Windows.Navigation;
//using Microsoft.Phone.Controls;
//using Microsoft.Phone.Shell;
//using phonePaint.Resources;
//using System.Windows.Media;
//using Microsoft.Phone.Info; 
//using System.Windows.Threading; 
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;

using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace LayerPaint
{
    public abstract class IGrap : DependencyObject
    {
        public byte[] OutByte;// { get; set; }
        //public abstract bool isBusy { get; }
        //public Action onDrawEnd { get; set; }
        public WriteableBitmap bitmap;
        public Color Color { get; set; }

        public abstract int W { get; }// { return wb.PixelWidth; } }
        public abstract int H { get; }// { return wb.PixelHeight; } }
        public abstract void Invalidate();
        public abstract void SetBitmap(WriteableBitmap outBitmap);

        public virtual void clear() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="r">半径</param>
        /// <param name="b">硬度 边缘羽化</param>
        /// <param name="opacity">透明度</param> 
        public abstract void fillCircle(double x, double y, double r, double b, double a);
        public abstract void fillRect(int x, int y, int w, int h);
        //public abstract void DrawBerzier(Vec2 p0, Vec2 p1, Vec2 p2, float size, float hard, float alpha);

       // public void setMask(WriteableBitmap bmp)
       // {
       //     if (bmp.PixelWidth != W || bmp.PixelHeight != H)
       //     {
       //         bmp = IGrap.copyImg(bmp, W, H, 0, 0);
       //     }
       //     MaskByte = bmp.PixelBuffer.ToArray();
       // }

        //public virtual void startDraw() { }
        //public virtual void endDraw() { }


        /*
        /// <summary>
        ///  三次混合
        ///      d
        ///  pen   temp   
        ///   \  +  /  a 
        ///    otemp    source 
        ///       \   &  /
        /// 	   output
        /// 	      | 
        /// </summary>
        /// <param name="x">坐标</param>
        /// <param name="y">坐标</param>
        /// <param name="r">半径</param>
        /// <param name="b">硬度  </param>
        /// <param name="a">笔透明度 底色百分比 </param>
        /// <param name="d"> 每次层叠深度 (叠(旧   </param>
        public void __fillCircle(double x, double y, double r, double b, double a, double d)
        {
            double r2 = r * r;
            double _r = 1 / (r);         //极限优化
            double _bf = 1 / (1f - b); //极限优化
            double ap, bp;
            double X, Y;
            //var c = Color.toBGRAByte();//pen
            //byte A = c[3], R = c[2], G = c[1], B = c[0]; 
            byte A, R, G, B;
            Color.outBGRAByte(out B, out G, out R, out A);
            //opacity *= b / 2 + 0.5f;
            int w = W, h = H, sl = (int)din.Length;// -w * 4;
            int _l = (int)Math.Max(0, y - r + 1), len = (int)Math.Min(y + r + 1, h);
            int _s = (int)Math.Max(0, x - r + 1), slen = (int)Math.Min(x + r + 1, w);
            int dlen = (int)r * 2 * 4;

            for (int l = _l; l < len; l++)
            {
                int lp = (l * w + _s) * 4;
                if (lp + dlen > sl) break;
                Y = l - y;
                double YY = Y * Y;
                for (int s = _s, pos = lp; s < slen; s++)//s<slen 防止划出界
                {
                    X = s - x;
                    double XX = X * X;
                    double p2 = (XX + YY);
                    if (p2 < r2)
                    {
                        double p = Math.Sqrt(p2);
                        ap = d; //max 1.00% 内容填充透明度
                        bp = p * _r;   // mp t:0-1 %     这两行别乱搞 
                        if (bp > b)
                            ap *= 1 - (bp - b) * _bf;
                        ap *= 0.5f;
                        double _ap = 1 - ap;
                        double _a = 1 - a;
                        byte TA = (byte)(A * ap + dtmp[pos + 3] * _ap);
                        byte OA = (byte)(TA * a + din[pos + 3] * _a);
                 

                        dtmp[pos] = (byte)(B * ap + dtmp[pos] * _ap);
                        dout[pos] = (byte)(dtmp[pos] * a + din[pos] * _a);
                        pos++;
                        dtmp[pos] = (byte)(G * ap + dtmp[pos] * _ap);
                        dout[pos] = (byte)(dtmp[pos] * a + din[pos] * _a);
                        pos++;
                        dtmp[pos] = (byte)(R * ap + dtmp[pos] * _ap);
                        dout[pos] = (byte)(dtmp[pos] * a + din[pos] * _a);
                        pos++;
                        dtmp[pos] = TA;// (byte)(A * ap + dtmp[pos] * _ap);
                        dout[pos] = OA;// (byte)(dtmp[pos] * a + din[pos] * _a);
                        pos++;
                    }
                    else pos += 4;
                }
            }
        }
        */



        byte[] GetPixelColor(int x, int y)
        {
            byte[] tb = new byte[4];
            int l = (y * W + x) * 4;
            for (int i = 0; i < 4; i++)
                tb[i] = OutByte[l + i];
            return tb;
        }
        byte[] GetPixelColor(Stream stream, int x, int y)
        {
            byte[] tb = new byte[4];
            stream.Position = (y * W + x) * 4;
            stream.Read(tb, 0, 4);
            return tb;
        }

        void SetPixelColor(int x, int y, byte[] new_color)
        {
            int l = (y * W + x) * 4;
            for (int i = 0; i < 4; i++)
                OutByte[l + i] = new_color[i];
            /*
            var stream = Stream;
            stream.Position = (y * W + x) * 4;
            stream.Write(new_color, 0, 4);*/
        }

        struct V { public int x_offset, y_offset;}/**/
        static V[] direction_8 = new V[]  { 
                new V(){x_offset=-1,y_offset= 1}, new V(){x_offset=0,y_offset= 1},  new V(){x_offset=1,y_offset= 1},
                new V(){x_offset=-1,y_offset= 0},  /* ************************** */ new V(){x_offset=1,y_offset= 0}, 
                new V(){x_offset=-1,y_offset=-1}, new V(){x_offset=0,y_offset=-1},  new V(){x_offset=1,y_offset=-1}};
        static V[] direction_4 = new V[]  {  new V(){x_offset=0,y_offset= 1},
                new V(){x_offset=-1,y_offset= 0}, new V(){x_offset=1,y_offset= 0}, 
                new V(){x_offset=0,y_offset=-1},   };
        struct Vector2 { public int X, Y; public Vector2(int x, int y) { X = x; Y = y; } }
        /// <summary>
        /// 旧归递填充算法
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="old_color"></param>
        /// <param name="new_color"></param>
        void floodSeedFill(int x, int y, byte[] old_color, byte[] new_color)
        {
            if (BitConverter.ToInt32(GetPixelColor(x, y), 0) == BitConverter.ToInt32(old_color, 0))
            {
                SetPixelColor(x, y, new_color);
                for (int i = 0; i < direction_8.Length; i++)
                {
                    floodSeedFill(x + direction_8[i].x_offset,
                                 y + direction_8[i].y_offset, old_color, new_color);
                }
            }
        }
        /// <summary>
        /// 填充算法
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void floodFill(int x, int y,byte[] MaskByte)
        {
            //floodSeedFill(x, y + 1, GetPixelColor(x, y), ((Color)brush.Color).toBufferByte());
            //int ln = (int)Stream.Length;
            int w = W, h = H;
            // byte[] b = dout;// new byte[ln];
            // Stream.Position = 0;
            //Stream.Read(b, 0, ln);
            if (x < 0 || w - 1 < x) return;
            if (y < 0 || h - 1 < y) return;

            byte[] old_color = GetPixelColor(MaskByte, (y * w + x) * 4);
            byte[] new_color = Color.toBGRAByte();
            if (BitConverter.ToInt32(new_color, 0) != BitConverter.ToInt32(old_color, 0))
            {
                var p = new Stack<Vector2>();
                p.Push(new Vector2(x, y));
                while (p.Count != 0)
                {
                    Vector2 m = p.Pop();
                    foreach (var item in direction_4)
                    {
                        var _m = new Vector2(m.X + item.x_offset, m.Y + item.y_offset);

                        if (_m.X < 0 || w - 1 < _m.X) continue;
                        if (_m.Y < 0 || h - 1 < _m.Y) continue;

                        int tp = (_m.Y * w + _m.X) * 4;

                        if (BitConverter.ToInt32(GetPixelColor(MaskByte, tp), 0) == BitConverter.ToInt32(old_color, 0))
                        {
                            SetPixelColor(OutByte, tp, new_color);
                            SetPixelColor(MaskByte, tp, new_color);
                            p.Push(_m);
                        }
                    }
                }
                //Stream.Position = 0;
                //Stream.Write(b, 0, ln);
            }
        }
        void SetPixelColor(byte[] stream, int p, byte[] new_color)
        {
            int i = 0;//p = (y * wb.PixelWidth + x) * 4;
            stream[p + i] = new_color[i]; i++;
            stream[p + i] = new_color[i]; ; i++;
            stream[p + i] = new_color[i]; ; i++;
            stream[p + i] = new_color[i]; ; i++;
        }
        byte[] GetPixelColor(byte[] stream, int p)
        {
            byte[] tb = new byte[4];
            int i = 0;//, p = (y * wb.PixelWidth + x) * 4;
            tb[i] = stream[p + i]; i++;
            tb[i] = stream[p + i]; i++;
            tb[i] = stream[p + i]; i++;
            tb[i] = stream[p + i]; i++;
            return tb;
        }


        public static async Task<WriteableBitmap> copyImgAsync(RenderTargetBitmap img)
        {
            if (img == null) return null;
            int ww = img.PixelWidth;
            int hh = img.PixelHeight;
            var w = new WriteableBitmap(ww, hh);

            // GC.Collect();
            var buff = await img.GetPixelsAsync();
            var data = buff.AsStream();
            var sw = w.PixelBuffer.AsStream();
            await Task.Run(() => {
                byte[] b = new byte[data.Length];
                data.Read(b, 0, (int)data.Length);
                sw.Write(b, 0, (int)data.Length);
            });

            return w;
        }
        public static async Task<WriteableBitmap> copyImgAsync(WriteableBitmap img)
        {
            if (img == null) return null;
            await Task.Yield();//没用
            var w = new WriteableBitmap(img.PixelWidth, img.PixelHeight);
            // await img.SetSourceAsync(w.PixelBuffer.AsStream().AsRandomAccessStream());
            img.PixelBuffer.CopyTo(w.PixelBuffer);
            return w;
        }
        public static async Task copyImgAsync(WriteableBitmap win, WriteableBitmap wout, int offx, int offy)
        {
            if (win == null || wout == null) return;
            Stream si = win.PixelBuffer.AsStream();
            Stream so = wout.PixelBuffer.AsStream();
            int sw = win.PixelWidth;
            int sh = win.PixelHeight;
            int iw = wout.PixelWidth;
            int ih = wout.PixelHeight;
            await Task.Run(() => {
                byte[] bi = new byte[si.Length];
                byte[] bo = new byte[so.Length];
                si.Read(bi, 0, bi.Length);
                so.Read(bo, 0, bo.Length);

                float[] cin = new float[4];
                fillEx(sw, sh, iw, ih, offx, offy, (w4, i, o) => {
                    byte[] b = new byte[w4];
                    si.Position = i;
                    si.Read(b, 0, w4);
                    so.Position = o;
                    so.Write(b, 0, w4);
                });
            });
            wout.Invalidate();
            win.Invalidate();
        }

        public static WriteableBitmap clipImg(WriteableBitmap img, Rect nr)
        {
            var nb = new WriteableBitmap((int)Math.Ceiling(nr.Width), (int)Math.Ceiling(nr.Height));
            IGrap.copyImg(img, nb, -(int)Math.Floor(nr.X), -(int)Math.Floor(nr.Y));
            return nb;
        }

        public static WriteableBitmap copyImg(WriteableBitmap img)
        {
            if (img == null) return null;
            var w = new WriteableBitmap(img.PixelWidth, img.PixelHeight);
            img.PixelBuffer.CopyTo(w.PixelBuffer);
            return w;
        }
        public static WriteableBitmap copyImg(WriteableBitmap Img, int mw, int mh, int offx, int offy)
        {
            int w = Img.PixelWidth;
            int h = Img.PixelHeight;
            int width = offx > 0 ?
                Math.Max(w + offx, mw) :
                Math.Max(w, offx + mw);
            int height = offy > 0 ?
                Math.Max(h + offy, mh) :
                Math.Max(h, offy + mh);
            offx = (offx > 0 ? offx : 0);
            offy = (offy > 0 ? offy : 0);
            WriteableBitmap bmp = new WriteableBitmap(width, height);
            Stream inss = Img.PixelBuffer.AsStream();
            Stream outs = bmp.PixelBuffer.AsStream();
            for (int l = 0, len = Img.PixelHeight, s = Img.PixelWidth * 4; l < len; l++)
            {
                byte[] b = new byte[s];
                inss.Read(b, 0, s);
                outs.Position = (l * width + offx) * 4;
                outs.Write(b, 0, s);
            }
            return bmp;
        }
        public static void copyImg(WriteableBitmap win, WriteableBitmap wout, int offx, int offy)
        {
            if (win == null || wout==null) return;
            Stream I = win.PixelBuffer.AsStream();
            Stream O = wout.PixelBuffer.AsStream();
            fillEx(win, wout, offx, offy, (w4, i, o) => {
                byte[] b = new byte[w4];
                I.Position = i;
                I.Read(b, 0, w4);
                O.Position = o;
                O.Write(b, 0, w4);
            });
        }
        
        public static void addImg(WriteableBitmap win, WriteableBitmap wout, int offx, int offy)
        {
            addImg(win, wout, offx, offy, 1);
        }
        public static void addImg(WriteableBitmap win, WriteableBitmap wout, int offx, int offy, double opacity)
        {
            if (win == null || wout == null) return;
            byte[] bi = win.PixelBuffer.ToArray();
            byte[] bo = wout.PixelBuffer.ToArray();
            float[] cin = new float[4];
            fillEx(win, wout, offx, offy, (w4, ni, no) => {
                for (int p = 0, l = w4; p < l; p += 4)
                {
                    if (bi[ni + p + 3] == 0) { continue; }

                    Color.BGRA2ARGB(ref cin, ref bi, ni + p);// Color.FromBGRA(bi, ni + p).toARGBByte();
                    float ap = (float)opacity * bi[ni + p + 3] / 255.0f;
                    float _ap = 1 - ap;


                    bo[no + p + 3] = (byte)Math.Min(255, cin[0] * (float)opacity + bo[no + p + 3] * _ap);
                    for (int c = 0; c < 3; c++)
                        bo[no + p + c] = (byte)Math.Min(255, +cin[3 - c] * ap + bo[no + p + c] * _ap);

                }
            });
            wout.PixelBuffer.AsStream().Write(bo, 0, bo.Length);
        }
        public static void delImg(WriteableBitmap win, WriteableBitmap wout, int offx, int offy)
        {
            if (win == null || wout == null) return;
            byte[] bi = win.PixelBuffer.ToArray();
            byte[] bo = wout.PixelBuffer.ToArray();
            float[] cin = new float[4];
            fillEx(win, wout, offx, offy, (w4, ni, no) => {
                for (int p = 0, l = w4; p < l; p += 4)
                {
                    if (bi[ni + p + 3] == 0) {
                        continue;
                    }

                    for (int c = 0; c < 4; c++)
                        bo[no + p + c] = 0;
                }
            });
            wout.PixelBuffer.AsStream().Write(bo, 0, bo.Length);
        }

        public static async Task addImgAsync(WriteableBitmap win, WriteableBitmap wout, int offx, int offy, double opacity)
        {
            if (win == null || wout == null) return;
            var si = win.PixelBuffer.AsStream();
            int sw = win.PixelWidth;
            int sh = win.PixelHeight;
            var so = wout.PixelBuffer.AsStream();
            int iw = wout.PixelWidth;
            int ih = wout.PixelHeight;
            await Task.Run(() => {
                byte[] bi = new byte[si.Length];
                byte[] bo = new byte[so.Length];
                si.Read(bi, 0, bi.Length);
                so.Read(bo, 0, bo.Length);

                float[] cin = new float[4];
                fillEx(sw, sh, iw, ih, offx, offy, (w4, ni, no) => {
                    for (int p = 0, l = w4; p < l; p += 4)
                    {
                        if (bi[ni + p + 3] == 0) { continue; }
                        Color.BGRA2ARGB(ref cin, ref bi, ni + p);// Color.FromBGRA(bi, ni + p).toARGBByte();
                        float ap = (float)opacity * bi[ni + p + 3] / 255.0f;
                        float _ap = 1 - ap;

                        bo[no + p + 3] = (byte)Math.Min(255, cin[0] * (float)opacity + bo[no + p + 3] * _ap);
                        for (int c = 0; c < 3; c++)
                            bo[no + p + c] = (byte)Math.Min(255, +cin[3 - c] * ap + bo[no + p + c] * _ap);
                    }
                });
                so.Position = 0;
                so.Write(bo, 0, bo.Length);
            });
            wout.Invalidate();
            win.Invalidate();
        }


        public static WriteableBitmap scanlImg(WriteableBitmap img, double sc)
        {
            WriteableBitmap d = new WriteableBitmap((int)(img.PixelWidth * sc), (int)(img.PixelHeight * sc));

            Stream sou = d.PixelBuffer.AsStream();
            byte[] bou = new byte[d.PixelBuffer.Length];

            Stream sin = img.PixelBuffer.AsStream();
            byte[] bin = new byte[img.PixelBuffer.Length];
            sin.Read(bin, 0, bin.Length);

            int len = (int)(d.PixelHeight);
            int slen = (int)(d.PixelWidth), _slen = (int)(img.PixelWidth);
            for (int y = 0; y < len; y++)
                for (int x = 0; x < slen; x++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var a = (y * slen + x) * 4 + i;
                        var b = ((y / sc) * _slen + (x / sc)) * 4 + i;
                        bou[a] = bin[(int)(b)];
                    }
                }

            sou.Write(bou, 0, (int)bou.Length);//已经优化
            d.Invalidate();
            return d;
        }

        public static void fillEx(WriteableBitmap In, WriteableBitmap Out, int offx, int offy, Action<int, int, int> fun)
        {
            int sw = In.PixelWidth;
            int sh = In.PixelHeight;
            int iw = Out.PixelWidth;
            int ih = Out.PixelHeight;
            fillEx(sw, sh, iw, ih, offx, offy, fun);
        }

        public static void fillEx(int sw, int sh, int iw, int ih, int offx, int offy, Action<int, int, int> fun)
        {
            int sx, sy, ix, iy, w, h;
            if (offx > 0)
            {
                sx = 0; ix = offx; w = Math.Min(iw - offx, sw);
            }
            else
            {
                sx = -offx; ix = 0; w = Math.Min(sw + offx, iw);
            }
            if (offy > 0)
            {
                sy = 0; iy = offy; h = Math.Min(ih - offy, sh);
            }
            else
            {
                sy = -offy; iy = 0; h = Math.Min(sh + offy, ih);
            }
            if (w < 0 || h < 0) return; //BUG
            for (int l = 0, w4 = w * 4; l < h; l++)
            {
                int ss = ((l + sy) * sw + sx) * 4;
                int ii = ((l + iy) * iw + ix) * 4;
                fun(w4, ss, ii);
            }

        }
        public static void clearImg(WriteableBitmap pwb)
        {
            Stream stream = pwb.PixelBuffer.AsStream();
            byte[] b = new byte[pwb.PixelBuffer.Length];
            stream.Write(b, 0, (int)b.Length); 
            pwb.Invalidate();
        }

        public static void fillColor(WriteableBitmap pwb, Func<int, int, Color> cb)
        {
            Stream stream = pwb.PixelBuffer.AsStream();
            byte[] b = new byte[pwb.PixelBuffer.Length];
            int i = 0, len = (int)(pwb.PixelHeight), slen = (int)(pwb.PixelWidth);
            for (int y = 0; y < len; y++)
                for (int x = 0; x < slen; x++, i += 4)
                    cb(x, y).outBGRAByte(ref b, i);
            stream.Write(b, 0, (int)b.Length);//已经优化
            pwb.Invalidate();
        }
        public static async Task fillColorAsync(WriteableBitmap pwb, Func<int, int, Color> cb)
        {
            Stream stream = pwb.PixelBuffer.AsStream();
            var bl = pwb.PixelBuffer.Length;
            int i = 0, len = (int)(pwb.PixelHeight), slen = (int)(pwb.PixelWidth);
            await Task.Run(() => {
                byte[] b = new byte[bl];
                for (int y = 0; y < len; y++)
                    for (int x = 0; x < slen; x++, i += 4)
                        cb(x, y).outBGRAByte(ref b, i);
                stream.Write(b, 0, (int)b.Length);//已经优化
            });
            pwb.Invalidate();
        }

        public void drawLine(Vec2 m, Vec2 p, float size, float hard, float opacity)
        {
            var v = new Vec2(-p.x + m.x, -p.y + m.y).Normalize();
            fillCircle((m.x - v.x * 0), (m.y - v.y * 0), size, hard, opacity);
            for (float sn = size * Math.Max(hard, 0.1f), i = sn, length = (float)Vec2.getLen(m, p); i < length; i += sn)
                //for (float sn = size * Math.Max(0.8f - hard, 0.3f), i = sn, length = (float)Vec2.getLen(m, p); i < length; i += sn)
                fillCircle((m.x - v.x * i), (m.y - v.y * i), size, hard, opacity);
        }

        //DEBUG
        public Vec2 getBerzier(Vec2 p0, Vec2 p1, Vec2 p2, float t)
        {
            /*
            p0:0,  p1:0,   p2:400,
            -------------------------
            t=0~1   x=0~400
            -------------------------
            0.1     0
            0.2     4.00000028312206
            0.3     16.0000011324883
            0.4     36.0000014305115
            0.5     64.000004529953
            0.6     100
            0.7     144.000005722046
            0.8     196.000027656555
            0.9     256.00004196167
            1.0     324.000072479248
            -------------------------
            */
            //t -= (float)(Math.Sin(t * 2 * 3.1415926f)) * 0.03f;
            var x = (1 - t) * (1 - t) * p0.x + 2 * t * (1 - t) * p1.x + t * t * p2.x;
            var y = (1 - t) * (1 - t) * p0.y + 2 * t * (1 - t) * p1.y + t * t * p2.y;
            return new Vec2(x, y);
        }


    }



}