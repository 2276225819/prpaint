using System;
using System.Collections.Generic;
using System.IO; 
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation; 
using System.Net;
using System.Windows; 
//using System.Windows.Controls;
//using System.Windows.Navigation;
//using Microsoft.Phone.Controls;
//using Microsoft.Phone.Shell;
//using phonePaint.Resources;
using System.Windows.Input;
//using System.Windows.Media;
//using Microsoft.Phone.Info; 
//using System.Windows.Threading; 
using System.Diagnostics;
using Windows.Storage.Pickers;
using Windows.UI.Input;
using Windows.ApplicationModel.Activation; 
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;

namespace LayerPaint 
{

    public class GrapAsync : IGrap
    {
        int width, height;
        Color cc; 
        WriteableBitmap bmp;
         
        
        public GrapAsync(WriteableBitmap wh)
        {
            setBitmap(wh);
        }
        public GrapAsync(int w, int h)
        { 
            width = w;
            height = h;
            OutByte = new byte[w * 4 * h];  
        }
         

        public override int W
        {
            get { return width; }
        }
        public override int H
        {
            get { return height; }
        }
        public override void setBitmap(WriteableBitmap outBitmap)
        {
            int w=1,h=1;
            if (outBitmap==null)return;
            { 
                w = outBitmap.PixelWidth;
                h = outBitmap.PixelHeight;
            }
            if (w != width || h != height)
            { 
                width = w;
                height = h;
                OutByte = new byte[w * 4 * h]; // outBitmap.PixelBuffer.ToArray();//
            } 
            bmp = outBitmap;
        }
        public override void clear()
        {
            OutByte = new byte[width * 4 * height];
            this.Invalidate();
        }


        bool _inv = false;
        public override async void Invalidate()
        {
            if (_inv) return; _inv = true;
            if (bmp != null)
            {
                var stream = bmp.PixelBuffer.AsStream();
                await Task.Run(() => {
                    stream.Write(OutByte, 0, OutByte.Length);
                }); 
                bmp.Invalidate();
            }
            _inv = false;
        }
        public override void startDraw()
        {
            base.startDraw();
        }
        public override void endDraw()
        {
            if (onDrawEnd != null && tasknum == 0) onDrawEnd();
        }







        public override Color Color
        {
            get { return cc; }
            set { cc = value; } 
        }



        public override void fillCircle(double x, double y, double r, double b, double a)
        {
            double r2 = r * r;
            double _r = 1 / (r);         //极限优化
            double _bf = 1 / (1f - b); //极限优化
            double ap, bp;
            double X, Y;
            //var c = Color.toBGRAByte();//pen
            //byte A = c[3], R = c[2], G = c[1], B = c[0]; 
            byte A, R, G, B;
            cc.outBGRAByte(out B, out G, out R, out A);
            //opacity *= b / 2 + 0.5f;
            int w = W, h = H, sl = (int)OutByte.Length;// -w * 4;
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
                    //if (OutByte[pos + 3] == 0)  continue; //bai hu tou ming se
                    X = s - x;
                    double XX = X * X;
                    double p2 = (XX + YY);
                    if (p2 < r2)
                    {
                        double p = Math.Sqrt(p2);
                        ap = a; //max 1.00% 内容填充透明度
                        bp = p * _r;   // mp t:0-1 %     这两行别乱搞 
                        if (bp > b)
                            ap *= 1 - (bp - b) * _bf;
                        ap *= 0.5f;
                        double _ap = 1 - ap; 
             
                        byte TA = (byte)(A * ap + OutByte[pos + 3] * _ap);

                        OutByte[pos] = (byte)(B * ap + OutByte[pos] * _ap); 
                        pos++;
                        OutByte[pos] = (byte)(G * ap + OutByte[pos] * _ap); 
                        pos++;
                        OutByte[pos] = (byte)(R * ap + OutByte[pos] * _ap); 
                        pos++;
                        OutByte[pos] = TA;// (byte)(A * ap + dtmp[pos] * _ap); 
                        pos++;
                    }
                    else pos += 4;
                }
            }
        }
 
        public override void fillRect(int x, int y, int w, int h)
        {
            if (x < 0 || W - w < x) return;
            if (y < 0 || H - h < y) return;
            if (w > W - x) return;
            if (h > H - y) return; 
            /////////////////////////////////////////////////////// 
            int s = w * 4;
            var wwww = width; 

            byte ca, cr, cg, cb;
            cc.outBGRAByte(out cb, out cg, out cr, out ca); 
            for (int l = y; l < y + h; l++)
            {
                int pos = (l * wwww + x) * 4;
                for (int i = 0; i < s; )
                {
                    OutByte[pos + i++] = cb;
                    OutByte[pos + i++] = cg;
                    OutByte[pos + i++] = cr;
                    OutByte[pos + i++] = ca; 
                }
            } 
        }

        public const float Step = 0.1f;
        Vec2 mm = Vec2.Zero;
        public float vsize = 0;

        int tasknum = 0;
        /*
        public async void drawBerzier(Vec2 p0, Vec2 p1, Vec2 p2, float[] mod, int size)
        {
            tasknum++;
            await Task.Run(() => {
                int nnn = 0;
                var tmp = new Vec2();
                var pa = Vec2.getCen(p0, p1);
                var pb = Vec2.getCen(p1, p2);
                float sn = (float)(Vec2.getLen(p0, p1) + Vec2.getLen(p1, p2));
                float sep = 1 / (sn * 1f);
                float weight = size * Step;
                int r = (int)size;
                int s = r * 2;

                for (float i = 0; i < 1f; i += sep)
                {
                    Vec2 m = getBerzier(pa, p1, pb, i); 
                    tmp.fromSub(m, mm);
                    if (Math.Abs(tmp.x) > weight || Math.Abs(tmp.y) > weight)
                    {
                        fill(mod, size, (float)m.x, (float)m.y); 
                        mm = m;
                    }
                    else nnn++;
                }
                tasknum--;
            });
            this.Invalidate();
            if (tasknum == 0 && onDrawEnd != null)
            {
                onDrawEnd();
            }

        }*/

        public override async void drawBerzier(Vec2 p0, Vec2 p1, Vec2 p2, float size, float hard, float alpha)
        {
            tasknum++; 
            await Task.Run(() => {
                int nnn = 0;
                var tmp = new Vec2();
                var pa = Vec2.getCen(p0, p1);
                var pb = Vec2.getCen(p1, p2);
                float sn = (float)(Vec2.getLen(p0, p1) + Vec2.getLen(p1, p2));
                float sep = 1 / (sn * 1f);
                float weight = size * Step;
                int r = (int)size;
                int s = r * 2;

                for (float i = 0; i < 1f; i += sep)
                {
                    Vec2 m = getBerzier(pa, p1, pb, i);
                    //float x = (float)m.X - r, y = (float)m.Y - r;
                    tmp.fromSub(m, mm);
                    if (Math.Abs(tmp.x) > weight || Math.Abs(tmp.y) > weight)
                    {
                        //if (vsize < r)
                        {//
                            //vsize += (r * 0.01f);
                        }
                        //MainPage.write(DateTime.Now.Ticks.ToString());
                        fillCircle(m.x, m.y, size, hard, alpha);
                        //fillA(  sout, fCircle, s, x, y);
                        mm = m;
                    }
                    else nnn++;
                }
                tasknum--;
            });
            this.Invalidate(); 
            if (tasknum == 0 && onDrawEnd != null)
            {
                onDrawEnd();
            }
        }



     
    }
}
