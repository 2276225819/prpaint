using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml;
using Windows.UI; 
using System.IO;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using System; 
using Windows.Foundation;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.UI.Input;
using Windows.Storage.Streams;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using System.Threading;

namespace LayerPaint
{
    /*
    public struct Point
    {
        public int X, Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
        public static implicit operator Windows.Foundation.Point(Point r)
        {
            return new Windows.Foundation.Point(r.X, r.Y);
        }
        public static implicit operator Point(Windows.Foundation.Point r)
        {
            return new Point((int)r.X, (int)r.Y);
        }
    }
    public struct Rect {
        public bool isEmpty { get { return W == 0 || H == 0; } }
        public int X, Y, W, H;
        public int Width { get { return W; } }
        public int Height { get { return H; } }
        public Rect(Vec2 p)
        {
            X = (int)p.X; Y = (int)p.Y; W = H = 0;
        }
        public Rect(Vec2 p, int rad)
        {
            X = (int)p.X - rad;
            Y = (int)p.Y - rad;
            W = rad*2;
            H = rad*2;
        }
        public Rect(FrameworkElement i)
        {
            if (i == null)
            {
                X = Y = W = H = 0; 
            } 
            var r = i.Margin;
            X = (int)r.Left;
            Y = (int)r.Top; 
            W = (int)i.RenderSize.Width;
            H = (int)i.RenderSize.Height;
        }
        public Rect(Size s)
        {
            X = Y = 0;
            W = (int)s.Width;
            H = (int)s.Height;
        }
        public Rect(Point p, Size s)
        {
            X = (int)p.X; 
            Y = (int)p.Y;
            W = (int)s.Width;
            H = (int)s.Height;
        }
        public Rect(Thickness t, int w,int h)
        {
            X = (int)t.Left;
            Y = (int)t.Top;
            W = w;
            H = h;

        }
        public Thickness toMargin()
        {
            return new Thickness(X,Y,0,0);
        }

        public Vec2 Center
        {
            get
            {
                return new Vec2(X + W / 2, Y + H / 2);
            }
        }
        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", X, Y, W, H);
        } 
        public Rect addRect(Rect r,Size s)
        { 
            int ox = X, oy = Y;
            X = Math.Max(0, Math.Min(X, r.X));//(int)p.X - size - 2));
            Y = Math.Max(0, Math.Min(Y, r.Y));//(int)p.Y - size - 2));
            W = (int)Math.Min(Math.Max(r.X + r.W, W + ox) - X, s.Width);
            H = (int)Math.Min(Math.Max(r.Y + r.W, H + oy) - Y, s.Height);
            return this;
        } 

        public static Windows.Foundation.Rect union(Windows.Foundation.Rect a, Windows.Foundation.Rect b)
        {
            return RectHelper.Union(a, b); 
        }
        public static Windows.Foundation.Rect intersect(Windows.Foundation.Rect a, Windows.Foundation.Rect b)
        {
            return RectHelper.Intersect(a, b);
        }
        
        public static implicit operator Windows.Foundation.Rect(Rect r)
        {
            return new Windows.Foundation.Rect(r.X, r.Y, r.W, r.H);
        }
        public static implicit operator Rect(Windows.Foundation.Rect r)
        {
            if (r.IsEmpty) return new Rect(); 
            return new Rect() { 
                X = (int)r.X, Y = (int)r.Y, 
                W = (int)r.Width,  H = (int)r.Height 
            }; 
        }
        public bool contains(int x,int y) {
            //return RectHelper.Contains(this, new Point(x, y));
            return X < x && x < X + W && Y < y && y < Y + H;
        }

        //public static Rect DrawRect
        //{
        //    get
        //    {
        //        return new Rect(Layers.draw.RenderSize);
        //    }
        //}
    }*/


    class Img
    {
        public static WriteableBitmap Create(double width, double height)
        {
            int w = (int)width;
            int h = (int)height;
            return Create(w, h);
        }
        public static WriteableBitmap Create(int w, int h)
        {
            var wn = new WriteableBitmap(w, h);
            //new Graphics(Color.Transparent, wn).fillRect(0, 0, w, h);
            return wn;
        }
        public static WriteableBitmap Create(ISize s)
        {
            var wn = new WriteableBitmap(s.DrawWidth, s.DrawHeight);
            //new Graphics(Color.Transparent, wn).fillRect(0, 0, w, h);
            return wn;
        }

        public static WriteableBitmap Create(string url)
        {
            var file = StorageFile.GetFileFromApplicationUriAsync(new Uri(url)).GetAwaiter().GetResult();
            var stream = file.OpenReadAsync().GetAwaiter().GetResult();
            var b = new WriteableBitmap(1, 1);
            b.SetSource(stream);
            b.PixelBuffer.ToArray();////BUG
            return b; 
        }

        public static async Task<WriteableBitmap> CreateAsync(StorageFile file)
        {
            var stream = await file.OpenReadAsync();
            var b = new WriteableBitmap(1, 1);//BUG
            b.SetSource(stream);
            b.PixelBuffer.ToArray();////BUG
            return b;
        }

        public static async Task<WriteableBitmap> CreateAsync(RandomAccessStreamReference file)
        {
            var stream = await file.OpenReadAsync();
            var b = new WriteableBitmap(1, 1);//BUG
            b.SetSource(stream);
            b.PixelBuffer.ToArray();////BUG
            return b;
        }



    }

    public interface ISize
    {
        int DrawWidth { get; }
        int DrawHeight { get; }
    }
    static class TT
    {
        public static Color getPixel(this WriteableBitmap me, int pos)
        {
            var str = me.PixelBuffer.AsStream();
            str.Position = pos * 4; 
            return Color.FromBGRA(
                (byte)str.ReadByte(), 
                (byte)str.ReadByte(),
                (byte)str.ReadByte(), 
                (byte)str.ReadByte());
        }
        public static void setPixel(this WriteableBitmap me, int pos, byte[] b)
        {
            if (pos < 0 || pos > me.PixelBuffer.Length) return;
            var str = me.PixelBuffer.AsStream();
            str.Position = pos * 4; 
            str.WriteByte(b[0]);//B
            str.WriteByte(b[1]);//G
            str.WriteByte(b[2]);//R
            str.WriteByte(b[3]);//A
        }
        public static void setPixel(this WriteableBitmap me, int pos, Color c)
        {
            me.setPixel(pos,c.toBGRAByte());
        }
        public static void setColor(this WriteableBitmap me, int x, int y, Windows.UI.Color c)
        { 
            int i = (int)(me.PixelWidth * y + x);
            me.setPixel(i, ((Color)c).toBGRAByte());
        }
        public static Color getColor(this WriteableBitmap me, int x, int y )
        { 
            int i = (int)(me.PixelWidth * y + x);
            int w = me.PixelWidth;
            if (i >= 0 && i < me.PixelBuffer.Length && 0<x && x<w)
                return me.getPixel(i);
            else 
                return Color.White;
        }
        public static WriteableBitmap Clone(this WriteableBitmap img)
        {
            if (img == null) return null;
            var b = new WriteableBitmap(img.PixelWidth, img.PixelHeight);
            img.PixelBuffer.CopyTo(b.PixelBuffer);
            return b;
        }

        public static string getInfo(this PointerPoint e)
        { 
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            switch (e.PointerDevice.PointerDeviceType)
            {
                case Windows.Devices.Input.PointerDeviceType.Mouse:
                    sb.Append("Pointer type: mouse");
                    break;
                case Windows.Devices.Input.PointerDeviceType.Pen:
                    sb.Append("Pointer type: pen");
                    break;
                case Windows.Devices.Input.PointerDeviceType.Touch:
                    sb.Append("Pointer type: touch");
                    break;
                default:
                    sb.Append("Pointer type: n/a");
                    break;
            } 
            sb.Append(Environment.NewLine);
            sb.Append("PointerPressed " + e.PointerId);
            sb.Append(Environment.NewLine);
            var props = e.Properties;
            sb.Append("接触区域的边框: " + props.ContactRect.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("原始输入的边框: " + props.ContactRectRaw.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("触笔设备的筒状按钮是否按下: " + props.IsBarrelButtonPressed.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("输入是否已由指针设备取消: " + props.IsCanceled.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("输入是否来自橡皮擦: " + props.IsEraser.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("输入是否来自滚轮: " + props.IsHorizontalMouseWheel.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("指针是否在触摸屏的范围内: " + props.IsInRange.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("是否是反转的值: " + props.IsInverted.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("输入是否来自鼠标左键: " + props.IsLeftButtonPressed.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("输入是否来自鼠标中键: " + props.IsMiddleButtonPressed.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("输入是否来自鼠标右键: " + props.IsRightButtonPressed.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("输入是否来自主要指针: " + props.IsPrimary.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("第一个扩展按钮的按下状态: " + props.IsXButton1Pressed.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("第二个扩展按钮的按下状态: " + props.IsXButton2Pressed.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("指针施加到触摸屏上的力度（0.0-1.0）: " + props.Pressure.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("触摸是否被拒绝了: " + props.TouchConfidence.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("指针状态的更改类型: " + props.PointerUpdateKind.ToString()); // PointerUpdateKind 枚举：LeftButtonPressed, LeftButtonReleased 等等
            sb.Append(Environment.NewLine);
            sb.Append("指针设备相关的 Orientation: " + props.Orientation.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("指针设备相关的 Twist: " + props.Twist.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("指针设备相关的 XTilt: " + props.XTilt.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("指针设备相关的 YTiltYTilt: " + props.YTilt.ToString());
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }

        public static async Task<WriteableBitmap> Render(this FrameworkElement element)
        {
            while (true)
            {
                try
                {
                    RenderTargetBitmap rtb = new RenderTargetBitmap();
                    await rtb.RenderAsync(element);

                    var di = DisplayInformation.GetForCurrentView();
                    RenderTargetBitmap rt = new RenderTargetBitmap();
                    await rt.RenderAsync(element, (int)Math.Round(rtb.PixelWidth / di.RawPixelsPerViewPixel / di.RawPixelsPerViewPixel),
                                                  (int)Math.Round(rtb.PixelHeight / di.RawPixelsPerViewPixel / di.RawPixelsPerViewPixel)); 
                    var pixelBuffer = await rt.GetPixelsAsync();

                    var bmp = new WriteableBitmap((int)rt.PixelWidth, (int)rt.PixelHeight);
                    await bmp.PixelBuffer.AsStream().AsRandomAccessStream().WriteAsync(pixelBuffer);
                    //await Task.Delay(3000); 
                    return bmp;
                }
                catch (Exception e)
                {
                    Debug.Write(e); 
                }
                await Task.Delay(1000);
            }
        }
    }
    public struct Color
    {
        public byte A, R, G, B;
        public int ToHex() { return A << 24 | R << 16 | G << 8 | B; }
        public Color LabToRGB(int L, int a, int b)
        {
            // For the conversion we first convert values to XYZ and then to RGB 
            // Standards used Observer = 2, Illuminant = D65 

            const double ref_X = 95.047;
            const double ref_Y = 100.000;
            const double ref_Z = 108.883;

            double var_Y = ((double)L + 16.0) / 116.0;
            double var_X = (double)a / 500.0 + var_Y;
            double var_Z = var_Y - (double)b / 200.0;

            if (Math.Pow(var_Y, 3) > 0.008856)
                var_Y = Math.Pow(var_Y, 3);
            else
                var_Y = (var_Y - 16 / 116) / 7.787;

            if (Math.Pow(var_X, 3) > 0.008856)
                var_X = Math.Pow(var_X, 3);
            else
                var_X = (var_X - 16 / 116) / 7.787;

            if (Math.Pow(var_Z, 3) > 0.008856)
                var_Z = Math.Pow(var_Z, 3);
            else
                var_Z = (var_Z - 16 / 116) / 7.787;

            double X = ref_X * var_X;
            double Y = ref_Y * var_Y;
            double Z = ref_Z * var_Z;

            return XYZToRGB(X, Y, Z);
        }
        public Color XYZToRGB(double X, double Y, double Z)
        {
            // Standards used Observer = 2, Illuminant = D65 
            // ref_X = 95.047, ref_Y = 100.000, ref_Z = 108.883 

            double var_X = X / 100.0;
            double var_Y = Y / 100.0;
            double var_Z = Z / 100.0;

            double var_R = var_X * 3.2406 + var_Y * (-1.5372) + var_Z * (-0.4986);
            double var_G = var_X * (-0.9689) + var_Y * 1.8758 + var_Z * 0.0415;
            double var_B = var_X * 0.0557 + var_Y * (-0.2040) + var_Z * 1.0570;

            if (var_R > 0.0031308)
                var_R = 1.055 * (Math.Pow(var_R, 1 / 2.4)) - 0.055;
            else
                var_R = 12.92 * var_R;

            if (var_G > 0.0031308)
                var_G = 1.055 * (Math.Pow(var_G, 1 / 2.4)) - 0.055;
            else
                var_G = 12.92 * var_G;

            if (var_B > 0.0031308)
                var_B = 1.055 * (Math.Pow(var_B, 1 / 2.4)) - 0.055;
            else
                var_B = 12.92 * var_B;

            int nRed = (int)(var_R * 256.0);
            int nGreen = (int)(var_G * 256.0);
            int nBlue = (int)(var_B * 256.0);

            if (nRed < 0) nRed = 0;
            else if (nRed > 255) nRed = 255;
            if (nGreen < 0) nGreen = 0;
            else if (nGreen > 255) nGreen = 255;
            if (nBlue < 0) nBlue = 0;
            else if (nBlue > 255) nBlue = 255;

            return Color.FromArgb(nRed, nGreen, nBlue);
        }
        public Color CMYKToRGB(double C, double M, double Y, double K)
        {
            int nRed = (int)((1.0 - (C * (1 - K) + K)) * 255);
            int nGreen = (int)((1.0 - (M * (1 - K) + K)) * 255);
            int nBlue = (int)((1.0 - (Y * (1 - K) + K)) * 255);

            if (nRed < 0) nRed = 0;
            else if (nRed > 255) nRed = 255;
            if (nGreen < 0) nGreen = 0;
            else if (nGreen > 255) nGreen = 255;
            if (nBlue < 0) nBlue = 0;
            else if (nBlue > 255) nBlue = 255;

            return Color.FromArgb(nRed, nGreen, nBlue);
        }

        /**/
        public byte[] toBGRAByte()
        {
            float max = A / 255f;
            return new byte[] { (byte)(B * max), (byte)(G * max), (byte)(R * max), A };
        }
        public byte[] toARGBByte()
        {
            return new byte[] { A, R, G, B };
        }

        public void outBGRAByte(out byte b, out byte g, out byte r, out byte a)
        {
            float max = A / 255f;
            b = (byte)(B * max);
            g = (byte)(G * max);
            r = (byte)(R * max);
            a = A;
        }
        public void outBGRAByte(ref byte[] b, int off)
        {
            float max = A / 255f;
            b[off + 0] = (byte)(B * max);
            b[off + 1] = (byte)(G * max);
            b[off + 2] = (byte)(R * max);
            b[off + 3] = A;

        }


        public static void BGRA2ARGB(ref float[] bout, ref byte[] wb, int off)
        { 
            float _a = 1.0f / wb[off + 3] * 255.0f;
            bout[0] = (wb[off + 3]);
            bout[1] = (wb[off + 2] * _a);
            bout[2] = (wb[off + 1] * _a);
            bout[3] = (wb[off + 0] * _a);
        }

        public static int ToHex(int R, int G, int B) { return (255 << 24 | R << 16 | G << 8 | B); }
        public static int ToHex(byte A, byte R, byte G, byte B) { return A << 24 | R << 16 | G << 8 | B; }
        public static int ToPix(byte A, byte R, byte G, byte B)
        {
            float max = A / 255f;
            return A << 24 | (byte)(max * R) << 16 | (byte)(max * G) << 8 | (byte)(max * B);
        }


        /// <summary>
        /// RGB范围 0~A 因为颜色 rgb最大值不是255所以要转换
        /// </summary>
        /// <param name="wb"> writebitmap数组 </param>
        /// <returns></returns> 
        public static Color FromBGRA(byte b, byte g, byte r, byte a)
        {
            //if (a == 0)  return Color.Transparent;
            //else
            {
                float _a = 1.0f / a * 255.0f;
                return new Color() {
                    A = (byte)(a),
                    B = (byte)Math.Round(b * _a),
                    G = (byte)Math.Round(g * _a),
                    R = (byte)Math.Round(r * _a)
                };
            }
        }
        public static Color FromBGRA(byte[] wb, int off = 0)
        {
            return Color.FromBGRA(wb[off + 0], wb[off + 1], wb[off + 2], wb[off + 3]);
            /*
            if (wb[3] == 0)
                return Color.Transparent;
            else
            {
                float _a = 1.0f / wb[off+3] * 255.0f;
                return new Color() {
                    B = (byte)(wb[off + 0] * _a),
                    G = (byte)(wb[off + 1] * _a),
                    R = (byte)(wb[off + 2] * _a),
                    A = (byte)(wb[off + 3]),
                };
            }*/
        }

        /// <summary>
        /// RGB范围 0~255
        /// </summary>
        /// <param name="b"> color数组 </param>
        /// <returns></returns>
        public static Color FromARGB(byte[] b)
        {
            return new Color() { A = b[0], R = b[1], G = b[2], B = b[3] };
        }
        public static Color FromARGB(byte a, byte r, byte g, byte b)
        {
            return new Color() { A = a, R = r, G = g, B = b };
        }

        public static Color FromArgb(double a, double r, double g, double b)
        {
            return new Color() { A = (byte)a, R = (byte)r, G = (byte)g, B = (byte)b };
        }
        public static Color FromArgb(float r, float g, float b)
        {
            return new Color() { A = 255, R = (byte)r, G = (byte)g, B = (byte)b };
        }
        public static Color FromArgb(int r, int g, int b)
        {
            return new Color() { A = 255, R = (byte)r, G = (byte)g, B = (byte)b };
        }
        public static Color FromHex(int hex)
        {
            return new Color() {
                A = (byte)(hex >> 24),
                R = (byte)(hex >> 16),
                G = (byte)(hex >> 8),
                B = (byte)(hex)
            };
        }

        public float H
        {
            get
            {
                byte[] rgb = new byte[] { R, G, B };
                Array.Sort(rgb);
                int max = rgb[2];
                int min = rgb[0];
                float hsbH = 0;
                if (max == R && G >= B)
                {
                    hsbH = (G - B) * 60f / (max - min) + 0;
                }
                else if (max == R && G < B)
                {
                    hsbH = (G - B) * 60f / (max - min) + 360;
                }
                else if (max == G)
                {
                    hsbH = (B - R) * 60f / (max - min) + 120;
                }
                else if (max == B)
                {
                    hsbH = (R - G) * 60f / (max - min) + 240;
                }
                if (float.IsNaN(hsbH))
                    return 0;
                else
                    return hsbH;
            }
        }
        public float S
        {
            get
            {
                byte[] rgb = new byte[] { R, G, B };
                Array.Sort(rgb);
                int max = rgb[2];
                int min = rgb[0];
                float hsbS = max == 0 ? 0 : (max - min) / (float)max;
                return hsbS;
            }
        }
        public float V
        {
            get
            {
                byte[] rgb = new byte[] { R, G, B };
                Array.Sort(rgb);
                int max = rgb[2];
                int min = rgb[0];
                float hsbB = max / 255.0f;
                return hsbB;
            }
        }

        /// <summary>
        /// aaa
        /// </summary>
        /// <param name="h">assert Float.compare(h, 0.0f  360.0f)  </param>
        /// <param name="s">assert Float.compare(s, 0.0f  1.0f)   </param>
        /// <param name="v">assert Float.compare(v, 0.0f  1.0f)   </param> 
        public static Color FromHSV(float h, float s, float v)
        {
            float r = 0, g = 0, b = 0;
            int i = (int)((h / 60.0f) % 6);
            float f = (h / 60.0f) - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);
            switch (i)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                case 5: r = v; g = p; b = q; break;
                default:
                    break;
            }
            return Color.FromArgb(r * 255.0f, g * 255.0f, b * 255.0f);
        }

        static float[] rgb2hsb(byte rgbR, byte rgbG, byte rgbB)
        {
            byte[] rgb = new byte[] { rgbR, rgbG, rgbB };
            Array.Sort(rgb);
            int max = rgb[2];
            int min = rgb[0];

            float hsbB = max / 255.0f;
            float hsbS = max == 0 ? 0 : (max - min) / (float)max;

            float hsbH = 0;
            if (max == rgbR && rgbG >= rgbB)
            {
                hsbH = (rgbG - rgbB) * 60f / (max - min) + 0;
            }
            else if (max == rgbR && rgbG < rgbB)
            {
                hsbH = (rgbG - rgbB) * 60f / (max - min) + 360;
            }
            else if (max == rgbG)
            {
                hsbH = (rgbB - rgbR) * 60f / (max - min) + 120;
            }
            else if (max == rgbB)
            {
                hsbH = (rgbR - rgbG) * 60f / (max - min) + 240;
            }

            return new float[] { hsbH, hsbS, hsbB };
        }

        static byte[] hsb2rgb(float h, float s, float v)
        {

            float r = 0, g = 0, b = 0;
            int i = (int)((h / 60) % 6);
            float f = (h / 60) - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);
            switch (i)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                case 5: r = v; g = p; b = q; break;
                default:
                    break;
            }
            return new byte[] {
                (byte) (r * 255.0), 
                (byte) (g * 255.0), 
                (byte) (b * 255.0)
            };
        }

        public static Color Black = Color.FromArgb(0, 0, 0);
        public static Color White = Color.FromArgb(255, 255, 255);
        public static Color Grey = Color.FromArgb(150, 150, 150);
        public static Color Transparent = Color.FromARGB(0, 255, 255, 255);
        public static Color Random
        {
            get
            {
                var r = new Random();
                return Color.FromArgb(r.Next(2) * 255, r.Next(2) * 255, r.Next(2) * 255);
            }
        }
        public void DEBUG()
        {
            Debug.WriteLine("#{0:x} 		{1}	{2}	{3}	{4}", ToHex(), A, R, G, B);
        }

        /// ////////////////////////////////////////////////////////

        public static implicit operator Windows.UI.Color(Color c)
        {
            return Windows.UI.Color.FromArgb(c.A, c.R, c.G, c.B);
        }
        public static implicit operator Color(Windows.UI.Color c)
        {
            return Color.FromARGB(c.A, c.R, c.G, c.B);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="l">顶层</param>
        /// <param name="r">底层</param>
        /// <returns></returns>
        public static Color operator +(Color l, Color r)
        {
            var _l = l.A / 255.0f;
            var _r = (1 - _l);//
            var _rr = _r * (r.A / 255.0f);// r.A == 0 ? 1 : _l / (r.A / 255.0f);
            var _ll = 1 - _rr;
            /*
            return new Color() {
                A = (byte)Math.Min(255, l.A + r.A * _r),
                R = l.R,
                G = l.G,
                B = l.B,
            };*/
            return new Color() {
                A = (byte)Math.Min(255, (l.A + r.A * _r)),
                R = (byte)(l.R * _ll + r.R * _rr),
                G = (byte)(l.G * _ll + r.G * _rr),
                B = (byte)(l.B * _ll + r.B * _rr),
            };/*
            return new Color() {
                A = (byte)Math.Min(255, l.A + r.A * _r),
                R = (byte)(l.R * _ll + r.R * _rr),
                G = (byte)(l.G * _ll + r.G * _rr),
                B = (byte)(l.B * _ll + r.B * _rr),
            };*/
        }

        public static Color FromData(string s)
        {
            return Color.FromHex(int.Parse(s.Substring(1), System.Globalization.NumberStyles.AllowHexSpecifier));
        }
        public override string ToString()
        {
            return string.Format("#{0:x}", ToHex());
        }
        public static string ToData(Color c)
        {
            return string.Format("#{0:x}", c.ToHex());

        }
    } 
    public struct Vec2
    {
        public static Vec2 Zero = new Vec2(0, 0);
        public double x, y;
        public double X { get { return x; } set { x = (float)value; } }
        public double Y { get { return y; } set { y = (float)value; } }
        /*
        public Vec2(double x, double y)
        {
            this.x = (float)x;
            this.y = (float)y;
        }*/
        public Vec2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public double Length
        {
            get { return Math.Sqrt(((X * X) + (Y * Y))); }
        }
        public Vec2 Normalize()
        {
            double l = Length;
            if (l == 0) return Vec2.Zero;
            this.X /= l;
            this.Y /= l;
            return this;
        }
        public Vec2 getNormalize()
        {
            double l = Length;
            if (l == 0) return Vec2.Zero;
            return new Vec2(this.X / l, this.Y / l);
        }
        //public Vec2 Normalize  {   get { return new Vec2(X, Y).Normalize(); }   } 
        public static Vec2 forAngle(double angle,double speed)
        {
           // return new Vec2(
            // Math.Round(speed * Math.Sin(angle * Math.PI / 180)) ,
            // Math.Round(speed * Math.Cos(angle * Math.PI / 180))  );


            return new Vec2(
             (speed * Math.Sin(angle * DEG2RAD)),
              (speed * Math.Cos(angle * DEG2RAD)));
        }
        public static Vec2 forAngle(double angle)
        {
            return new Vec2(Math.Sin(angle * DEG2RAD), Math.Cos(angle * DEG2RAD));
        }
        static double RAD2DEG = 180 / Math.PI;
        static double DEG2RAD = Math.PI / 180;
        public static double getAngle(double X, double Y)
        {
            var angle =  Math.Round(Math.Atan2(Y, X) * RAD2DEG);
            if (angle < 0) angle += 360;
            return angle;
        }
        /// <summary>
        /// 返回角度
        /// </summary>
        /// <returns>-180~180</returns>
        public double getAngle()
        {
            return Math.Round(Math.Atan2(Y, X) * RAD2DEG); 
        }
        public static Vec2 operator +(Vec2 l, Vec2 r)
        {
            return new Vec2(l.X + r.X, l.Y + r.Y);
        }
        public static Vec2 operator -(Vec2 l, Vec2 r)
        {
            return new Vec2(l.X - r.X, l.Y - r.Y);
        }
        public static Vec2 operator +(Vec2 l, Point r)
        {
            return new Point(l.X + r.X, l.Y + r.Y);
        }
        public static Vec2 operator -(Vec2 l, Point r)
        {
            return new Point(l.X - r.X, l.Y - r.Y);
        }

        public void fromSub(Vec2 l, Vec2 r)
        {
            x = l.x - r.x;
            y = l.y - r.y; 
        }
        public void fromAdd(Vec2 l, Vec2 r)
        {
            x = l.x + r.x;
            y = l.y + r.y;  
        }
        
        public static Vec2 operator *(Vec2 l, float f)
        {
            return new Vec2(l.X * f, l.Y * f);
        } 
        public static Vec2 operator *(Vec2 l, double f)
        {
            return new Vec2(l.X * f, l.Y * f);
        }

        public static bool operator ==(Vec2 l, Vec2 f)
        {
            return l.x == f.x && l.y == f.y;
        }
        public static bool operator !=(Vec2 l, Vec2 f)
        {
            return l.x != f.x && l.y != f.y;
        }




        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static implicit operator Windows.Foundation.Point(Vec2 c)
        {
            return new Windows.Foundation.Point(c.X, c.Y);
        }
        public static implicit operator Vec2(Windows.Foundation.Point c)
        {
            return new Vec2(c.X, c.Y);
        }


        public static double getAngle(Vec2 pSrc, Vec2 p1, Vec2 p2)
        {
            double angle = 0.0f; // 夹角

            // 向量Vector a的(x, y)坐标
            double va_x = p1.x - pSrc.x;
            double va_y = p1.y - pSrc.y;

            // 向量b的(x, y)坐标
            double vb_x = p2.x - pSrc.x;
            double vb_y = p2.y - pSrc.y;

            double productValue = (va_x * vb_x) + (va_y * vb_y);  // 向量的乘积
            double va_val = Math.Sqrt  (va_x * va_x + va_y * va_y);  // 向量a的模
            double vb_val = Math.Sqrt(vb_x * vb_x + vb_y * vb_y);  // 向量b的模
            double cosValue = productValue / (va_val * vb_val);      // 余弦公式

            // acos的输入参数范围必须在[-1, 1]之间，否则会"domain error"
            // 对输入参数作校验和处理
            if (cosValue < -1 && cosValue > -2)
                cosValue = -1;
            else if (cosValue > 1 && cosValue < 2)
                cosValue = 1;

            // acos返回的是弧度值，转换为角度值
            angle = Math.Acos(cosValue) * 180 / 3.1415926;

            return angle;
        }
        public override string ToString()
        {
            return X.ToString("f4") + "," + Y.ToString("f4");
        }
        public static double getLen(Vec2 o, Vec2 a)
        {
            return (o - a).Length;
        }
        public static bool testLen(Vec2 o,Vec2 a,double len)
        {
            return (o - a).Length > len;
            //return Math.Abs(o.x - a.x) > len || Math.Abs(o.y - a.y) > len; 
        }
        public static Vec2 getCen(Vec2 o, Vec2 a)
        {
            //return o * 0.5 - a * 0.5 + o;
            return (a - o) * 0.5 + o; 
        }
        public Vec2 toCen(Vec2 a)
        {
            x = (x - a.x) * 0.5f + x;
            y = (y - a.y) * 0.5f + y;
            return this;
        }
        public bool isEmpty() { return X == 0 && Y == 0; }
    }

     
}
 

///12313132344324