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
using App2.Model;

namespace LayerPaint
{ 
    //不常用
    public abstract partial class IGrap : DependencyObject
    {
        public float[] createCircle(int size, float hard, float opacity)
        {
            int r = size / 2;
            float r2 = r * r, _r = 1f / (r);
            float b = hard, _bf = 1f / (1f - b); //极限优化
            float[] data = new float[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int Y = y - r;
                    int X = x - r;
                    float p2 = (X * X + Y * Y);
                    if (p2 <= r2)
                    {
                        float p = (float)Math.Sqrt(p2);
                        float ap = opacity; //max 1.00% 内容填充透明度
                        float bp = p * _r;   // mp t:0-1 %     这两行别乱搞 
                        if (bp > hard)
                        {
                            ap *= 1 - (bp - b) * _bf;
                        }
                        data[y * size + x] = ap;
                    }
                    //else { data[y * size + x] = -1; }
                }
            }
            return data;
        }

        public void fillAlpha(Stream sin, Stream sout, float[] bAlpha, int size, float offx, float offy)
        {
            int sw = size;
            int sh = size;
            int iw = W;
            int ih = H;
            int sx, sy, ix, iy, w, h;
            if (offx > 0)
            {
                sx = 0; ix = (int)offx; w = (int)Math.Min(iw - offx, sw);
            }
            else
            {
                sx = (int)-offx; ix = 0; w = (int)Math.Min(sw + offx, iw);
            }
            if (offy > 0)
            {
                sy = 0; iy = (int)offy; h = (int)Math.Min(ih - offy, sh);
            }
            else
            {
                sy = (int)-offy; iy = 0; h = (int)Math.Min(sh + offy, ih);
            }
            if (w < 0 || h < 0)
                return;
            //var c = Color.toBGRAByte();
            //float A = c[3], R = c[2], G = c[1], B = c[0];
            byte B, G, R, A;
            Color.outBGRAByte(out B, out G, out R, out A);
            int w4 = w * 4;
            byte[] data = new byte[w4]; //  
            byte[] ssss = new byte[w4]; //  
            for (int l = 0; l < h; l++)
            {
                int bof = ((l + sy) * sw + sx);
                int smof = ((l + iy) * iw + ix) * 4;
                sout.Position = smof;   //shu cu
                sout.Read(data, 0, w4);
                for (int pos = 0, p = 0; p < w; p++)
                {
                    float ap = bAlpha[bof + p];
                    float _ap = 1 - ap;
                    // hua bi    di se
                    data[pos] = (byte)(B * ap + data[pos] * _ap); pos++;
                    data[pos] = (byte)(G * ap + data[pos] * _ap); pos++;
                    data[pos] = (byte)(R * ap + data[pos] * _ap); pos++;
                    data[pos] = (byte)(A * ap + data[pos] * _ap); pos++;

                }
                sout.Position = smof;
                sout.Write(data, 0, w4);
            }
        }
        public void fillA(Stream sout, float[] bAlpha, int size, float offx, float offy)
        {
            //var c = Color.toBGRAByte();
            //byte A = c[3], R = c[2], G = c[1], B = c[0];
            byte A, R, G, B;
            Color.outBGRAByte(out B, out G, out R, out A);
            fillEx(size, size, offx, offy, (w4, bof, smof) => {//错误 浮点强转为整形了
                byte[] data = new byte[w4]; // 
                int w = w4 / 4;
                sout.Position = smof;   //shu cu
                sout.Read(data, 0, w4);
                for (int pos = 0, p = 0; p < w; p++)
                {
                    float ap = bAlpha[bof + p];
                    float _ap = 1 - ap;
                    // hua bi    di se
                    data[pos] = (byte)(B * ap + data[pos] * _ap); pos++;
                    data[pos] = (byte)(G * ap + data[pos] * _ap); pos++;
                    data[pos] = (byte)(R * ap + data[pos] * _ap); pos++;
                    data[pos] = (byte)(A * ap + data[pos] * _ap); pos++;
                }
                sout.Position = smof;
                sout.Write(data, 0, w4);
            });
        }
        public void fillB(Stream sin, Stream sout, float[] bAlpha, int size, float offx, float offy)
        {
            size = size + 1;   //!!!!!!!!!!!!!!!!!!!!!

            byte A, R, G, B;
            Color.outBGRAByte(out B, out G, out R, out A);
            fillEx(size, size, offx, offy, (w4, bof, smof) => {
                byte[] data = new byte[w4]; // 
                int w = w4 / 4;
                sout.Position = smof;   //shu cu
                sout.Read(data, 0, w4);
                for (int pos = 0, p = 0; p < w; p++)
                {
                    float ap = bAlpha[bof + p];
                    float _ap = 1 - ap;
                    // hua bi    di se
                    data[pos] = B;// (byte)(B * ap + data[pos] * _ap); 
                    pos++;
                    data[pos] = G;//(byte)(G * ap + data[pos] * _ap); 
                    pos++;
                    data[pos] = R;//(byte)(R * ap + data[pos] * _ap); 
                    pos++;
                    data[pos] = A;// (byte)(A * ap + data[pos] * _ap);
                    pos++;
                }
                sout.Position = smof;
                sout.Write(data, 0, w4);
            });
        }

        void fillEx(int sw, int sh, float offx, float offy, Action<int, int, int> fun)
        {
            int iw = W;
            int ih = H;
            float sx, sy, ix, iy;
            int w, h;
            if (offx > 0)
            {
                sx = 0; ix = offx; w = (int)Math.Min(iw - offx, sw);
            }
            else
            {
                sx = -offx; ix = 0; w = (int)Math.Min(sw + offx, iw);
            }
            if (offy > 0)
            {
                sy = 0; iy = offy; h = (int)Math.Min(ih - offy, sh);
            }
            else
            {
                sy = -offy; iy = 0; h = (int)Math.Min(sh + offy, ih);
            }
            if (w < 0 || h < 0)
                return;
            int w4 = w * 4;
            for (int l = 0; l < h; l++)
            {
                int bof = (int)((l + sy) * sw + sx);
                int smof = (int)((l + iy) * iw + ix) * 4;
                fun(w4, bof, smof);
            }
        }

 
    }
     
 

}
