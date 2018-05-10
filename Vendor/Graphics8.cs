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
using System.Threading.Tasks.Dataflow;
using App2.Model;
using Windows.UI.Core;

namespace LayerPaint
{

    public class Graphics8 : IGrap
    {
        Task task;

        int ww, hh;
        public Graphics8(Color c, WriteableBitmap w)
        {
            task = Task.Run(delegate { });
            Color = c;
            SetBitmap(w);

        }
        public Graphics8() : this(Color.Black, new WriteableBitmap(1, 1)) { }
      
        public override int W
        {
            get { return ww; }
        }


        public override int H
        {
            get { return hh; }
        }



        bool _inv = false;
        public async override void Invalidate()
        {
            if (_inv || outStream == null) return;
            _inv = true;
            //var t = DateTime.Now; 
            await Task.Yield();
            task = task.ContinueWith(x => {
                //lock (OutByte)
                // {
                outStream.Position = 0;
                outStream.Write(OutByte, 0, OutByte.Length);

                Dispatcher.RunIdleAsync(_ => {
                    bitmap.Invalidate();
                    _inv = false;
                    //Debug.WriteLine(DateTime.Now - t);
                }).ToString();
                // }
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="outBitmap">新图层 用于输出 </param>
        /// <param name="inBitmap">旧图层 用于输入</param>
        public  override void SetBitmap(WriteableBitmap outBitmap)
        {
            bitmap = outBitmap;
            ww = outBitmap.PixelWidth;
            hh = outBitmap.PixelHeight;
            outStream = outBitmap.PixelBuffer.AsStream();
            task = task.ContinueWith(x => {
                //lock (OutByte)
                //{
                OutByte = new byte[(int)outStream.Length];
                outStream.Read(OutByte, 0, OutByte.Length);
                //MaskByte = (byte[])OutByte.Clone();
                //din = (byte[])OutByte.Clone();
                //dtmp = (byte[])OutByte.Clone();
                //}
            });
        }



        Stream outStream;
        //byte[] din;
        //byte[] dtmp;
        public unsafe void fillCircle(Color cc, double x, double y, double r, double b, double a, double d)
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
            int w = W, h = H, sl = OutByte.Length;// -w * 4;
            int _l = (int)Math.Floor(Math.Max(0, y - r - 1)), len = (int)Math.Ceiling(Math.Min(y + r + 1, h));
            int _s = (int)Math.Floor(Math.Max(0, x - r - 1)), slen = (int)Math.Ceiling(Math.Min(x + r + 1, w));
            int dlen = (int)r * 2 * 4;

            fixed (byte* _OutByte = OutByte)//, _din = din,_dtmp = dtmp)
            {
                for (int l = _l; l < len; l++)
                {
                    int lp = (l * w + _s) * 4;
                    if (lp + dlen > sl) break;
                    Y = l - y;
                    double YY = Y * Y;
                    byte* pOut = _OutByte + lp;
                    // byte* pDin = _din + lp;
                    // byte* pTmp = _dtmp + lp;
                    for (int s = _s; s < slen; s++)//s<slen 防止划出界
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
                            //double _a = 1 - a;


                            *pOut = (byte)(B * ap + *pOut * _ap); pOut++;
                            *pOut = (byte)(G * ap + *pOut * _ap); pOut++;
                            *pOut = (byte)(R * ap + *pOut * _ap); pOut++;
                            *pOut = (byte)(A * ap + *pOut * _ap); pOut++;
                            // *pTmp = (byte)(B * ap + *pTmp * _ap);
                            // *pOut = (byte)(*pTmp * a + *pDin * _a);
                            // pOut++; pDin++; pTmp++;
                            // *pTmp = (byte)(G * ap + *pTmp * _ap);
                            // *pOut = (byte)(*pTmp * a + *pDin * _a);
                            // pOut++; pDin++; pTmp++;
                            // *pTmp = (byte)(R * ap + *pTmp * _ap);
                            // *pOut = (byte)(*pTmp * a + *pDin * _a);
                            // pOut++; pDin++; pTmp++;
                            // *pTmp = (byte)(A * ap + *pTmp * _ap);
                            // *pOut = (byte)(*pTmp * a + *pDin * _a);
                            // pOut++; pDin++; pTmp++;
                        }
                        else
                        {
                            pOut += 4;
                            //pDin += 4; pTmp += 4;
                        }
                    }
                }
            }
        }

        public override void fillRect(int x, int y, int w, int h)
        {
            if (x < 0 || W - w < x) return;
            if (y < 0 || H - h < y) return;
            if (w > W - x) return;
            if (h > H - y) return;
            //x = Math.Max(0, Math.Min(wb.PixelWidth-w, x));
            //y = Math.Max(0, Math.Min(wb.PixelHeight-h, y));
            //w = Math.Min(w , wb.PixelWidth - x);
            //h = Math.Min(h , wb.PixelHeight - y);
            /////////////////////////////////////////////////////// 
            int s = w * 4;
            var ww = W;

            byte ca, cr, cg, cb;
            Color.outBGRAByte(out cb, out cg, out cr, out ca);
            for (int l = y; l < y + h; l++)
            {
                int pos = (l * ww + x) * 4;
                for (int i = 0; i < s;)
                {
                    OutByte[pos + i++] = cb;
                    OutByte[pos + i++] = cg;
                    OutByte[pos + i++] = cr;
                    OutByte[pos + i++] = ca;
                }
            }
            // dtmp = (byte[])OutByte.Clone();
            // din = (byte[])OutByte.Clone();
        }


        public Vec2 mm = Vec2.Zero;
        public const float Step = 0.1f;

        public float Size = 0;
        public Task DrawBerzier(Vec2 p0, Vec2 p1, Vec2 p2, float size, float hard, float alpha, float dep = 1f, float blend = 0f, float di = 0f)
        {
            return task = task.ContinueWith( _ => {
                //task = task.ContinueWith
                var tmp = new Vec2();
                var pa = Vec2.getCen(p0, p1);
                var pb = Vec2.getCen(p1, p2);
                float sn = (float)(Vec2.getLen(p0, p1) + Vec2.getLen(p1, p2));
                float sep = 1 / (sn * 1f);
                float weight = size * Step;

                //if (_lsize == 0) _lsize = size;
                var msize = Size + (Math.Min(Math.Max(Size * (sn / 30), size), size) - Size) * 0.2f;
                var lsize = Size;
                var spe = (msize - Size);
                Size = msize;

                /*/
               var lsize = size; 
                var spe = 1; /*/

                //var ts = new List<Task>();
                for (float i = 0; i < 1f; i += sep)
                {
                    Vec2 m = getBerzier(pa, p1, pb, i);
                    //float x = (float)m.X - r, y = (float)m.Y - r;
                    tmp.fromSub(m, mm);
                    if (Math.Abs(tmp.x) > weight || Math.Abs(tmp.y) > weight)
                    {
                        var cc = getBlendColor(Color, mm, m, (int)(lsize + (spe * i)), blend, di);
                        //ts.Add(Task.Run(() => {
                        //task.ContinueWith( __ => {
                            fillCircle(cc, m.x, m.y, lsize + (spe * i), hard, alpha, dep);//       
                            //Invalidate(); 
                        //});
                        //})); 
                        mm = m;
                    }
                } 
                //Task.WaitAll(ts.ToArray()); 
                Invalidate(); 
            });

        }

        public Task DrawCircle(Vec2 p0, float size, float hard, float alpha, float dep = 1f, float blend = 0f, float di = 0f)
        {
            return task = task.ContinueWith(_ => {
                var cc = getBlendColor(Color, p0, p0, (int)Size, blend, di);
                fillCircle(cc, p0.x, p0.y, (int)Size, hard, alpha, dep);//
                Invalidate();
            });
        }
        public Task DrawRect(int x, int y, int w, int h)
        {
            return task = task.ContinueWith(_ => {
                fillRect(x, y, w, h);
                Invalidate();
            });
        }

        //  public Color getBlendColor(Color inc, Vec2 mm, Vec2 m, int dr, double blend, double dilution)
        //  {
        //      var mohu = blend;
        //      var cm = mm;
        //      mm += (m - mm).getNormalize() * dr * 0.5;
        //    
        //      var l = Color.FromBGRA(OutByte, Math.Max(0, Math.Min(OutByte.Length - 4, ((int)cm.y * ww + (int)cm.x) * 4)));
        //      var r = Color.FromBGRA(OutByte, Math.Max(0, Math.Min(OutByte.Length - 4, ((int)mm.y * ww + (int)mm.x) * 4)));
        //
        //
        //
        //      var _rr = mohu;
        //      var _ll = (1 - _rr);            
        //      return new Color() {
        //          //A = (byte)Math.Round(l.A * (1 - dilution) + r.A * dilution),
        //          A = (byte)Math.Round(l.A * _ll + r.A * _rr),
        //          R = (byte)Math.Round(l.R * _ll + r.R * _rr),
        //          G = (byte)Math.Round(l.G * _ll + r.G * _rr),
        //          B = (byte)Math.Round(l.B * _ll + r.B * _rr),
        //      };
        //
        //  }

        public Color getBlendColor(Color l, Vec2 mm, Vec2 m, int dr, double blend, double dilution)
        {
            m += (m - mm).getNormalize() * dr * 0.5;
            //var rnd = new Random();
            var ix = (int)m.x;//+ rnd.Next(-dr, dr);
            var iy = (int)m.y;//+ rnd.Next(-dr, dr); 

            var r = Color.FromBGRA(OutByte, Math.Max(0, Math.Min(OutByte.Length - 4, (iy * ww + ix) * 4)));
            var _rr = blend * (r.A / 255d);
            var _ll = (1 - _rr);
            //if((l.R * _ll + r.R * _rr) > 40)
            //{ 
            //Debug.WriteLine(Math.Round(l.R * _ll + r.R * _rr));
            //}
            return new Color() {
                A = (byte)Math.Round(l.A * (1 - dilution) + r.A * dilution),
                R = (byte)Math.Round(l.R * _ll + r.R * _rr),
                G = (byte)Math.Round(l.G * _ll + r.G * _rr),
                B = (byte)Math.Round(l.B * _ll + r.B * _rr),
            };

        }

        public override void fillCircle(double x, double y, double r, double b, double a)
        {
            fillCircle(Color, x, y, r, b, a, 1);
        }
    }

}
