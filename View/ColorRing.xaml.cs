using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace App2.View
{
    public sealed partial class ColorRing : UserControl
    {
        public SolidColorBrush MainBrush { get; set; }

        int f;
        int r;
        int W;
        public WriteableBitmap pwb, swb;
        public ColorRing()
        {
            this.InitializeComponent();
            Dispatcher.RunIdleAsync((s) => {
                if (MainBrush == null) return;
                setColor(MainBrush.Color);
                MainBrush.RegisterPropertyChangedCallback(SolidColorBrush.ColorProperty, (obj, prop) => {
                    if (HoverElement == null)
                    {
                        setColor((Color)obj.GetValue(prop));
                    }
                });
            }).ToString();
            

            DataContext = this;
            r = (int)(BG.Width);
            f = (int)(PickSel.Width) / 2;
            Pick.Width = (W = r / 2);
            // BrushSel = new SolidColorBrush(Color.Black);
            //W = (int)(r - Pick.Margin.Left * 2);

            pwb = new WriteableBitmap(W / 4, W / 4);
            swb = new WriteableBitmap(r * 2, r * 2);


            Slide.Background = new ImageBrush() { ImageSource = swb };
            Pick.Background = new ImageBrush() { ImageSource = pwb };
            drawPick();//DEBUG
            drawSlide();//DEBUG
            init();
        }

        int angle = 0;
        bool loc;
        Canvas HoverElement;
        public void init()
        {

            // CA.ManipulationDelta += Released;
            //  CR.ManipulationDelta += Released;
            //  CG.ManipulationDelta += Released;
            //  CB.ManipulationDelta += Released;

            Pick.PointerPressed += (s, e) => {
                if (e.Pointer.IsInContact)
                {
                    if (HoverElement == null)
                    {
                        HoverElement = Pick;
                        Point p = e.GetCurrentPoint(Pick).Position;
                        p.X = Math.Max(Math.Min(p.X, W), 0);
                        p.Y = Math.Max(Math.Min(p.Y, W), 0);
                        setColor(angle, p.X / W, 1 - p.Y / W);
                    } 
                }
            };
            Slide.PointerPressed += (s, e) => {
                if (e.Pointer.IsInContact)
                {
                    if (HoverElement == null)
                    {
                        HoverElement = Slide;
                        Point p = e.GetCurrentPoint(Slide).Position;//圈圈坐标 
                        double x = (double)PickSel.GetValue(Canvas.LeftProperty) + f;
                        double y = (double)PickSel.GetValue(Canvas.TopProperty) + f;
                        setColor(LayerPaint.Vec2.getAngle(p.X - W, p.Y - W), x / W, 1 - y / W);
                    } 
                }
            };
            PointerReleased += (s, e) => {
                MainBrush.Color = (PickSel.Fill as SolidColorBrush).Color;
                HoverElement = null; 
            };
            PointerMoved += (s, e) => {
                if (HoverElement == Slide)//圈圈
                {
                    Point p = e.GetCurrentPoint(Slide).Position;//圈圈坐标 
                    double x = (double)PickSel.GetValue(Canvas.LeftProperty) + f;
                    double y = (double)PickSel.GetValue(Canvas.TopProperty) + f;
                    setColor(LayerPaint.Vec2.getAngle(p.X - W, p.Y - W), x / W, 1 - y / W); 

                };
                if (HoverElement == Pick)//方方
                {
                    Point p = e.GetCurrentPoint(Pick).Position;
                    p.X = Math.Max(Math.Min(p.X, W), 0);
                    p.Y = Math.Max(Math.Min(p.Y, W), 0);
                    setColor(angle, p.X / W, 1 - p.Y / W);
                }
                if (HoverElement != null && !loc)
                {
                    loc = true;
                    Dispatcher.RunIdleAsync(ss => {
                        MainBrush.Color = (PickSel.Fill as SolidColorBrush).Color;
                        loc = false;
                    }).ToString();

                }
            };
        }

        // void Released(object sender, ManipulationDeltaRoutedEventArgs e)
        // {
        //     if (e.IsInertial) e.Complete();
        //     // setColor(Color.FromArgb(CA.Value, CR.Value, CG.Value, CB.Value));
        //     // setColor(Color.FromArgb(255, CR.Value, CG.Value, CB.Value));
        // }

        // public SolidColorBrush BrushDef
        // {
        //     get { return ColorDef.Background as SolidColorBrush; }
        //     set { ColorDef.Background = value; setColor(value.Color); }
        // }
        // public SolidColorBrush BrushSel
        // {
        //     get { return ColorSel.Background as SolidColorBrush; }
        //     set { ColorSel.Background = value; }
        // }

        public void set(LayerPaint.Color c, float h, float s, float b)
        {
            // BrushSel.Color = c;
            // //CA.Value = c.A;
            // CR.Value = c.R;
            // CG.Value = c.G;
            // CB.Value = c.B;

            int cr = (r - (int)SlideSel.Width) / 2;
            //修改角度
            var vp = LayerPaint.Vec2.forAngle(90 - h, cr);
            //SlideSel.Margin = new Thickness(vp.X, vp.Y, 0, 0);
            SlideSel.SetValue(Canvas.LeftProperty, (vp.X) + cr);
            SlideSel.SetValue(Canvas.TopProperty, (vp.Y) + cr);
            //修改坐标 
            PickSel.SetValue(Canvas.LeftProperty, (s) * W - f);
            PickSel.SetValue(Canvas.TopProperty, (1 - b) * W - f);
            angle = (int)h;

            (PickSel.Fill as SolidColorBrush).Color = c;

            /*
            g.Color = c;
            g.fillRect(0, 0, 10, 10);
            g.Invalidate(); */
            drawPick();
        }
        public void setColor(double h, double s, double b)
        {
            float H = (float)h, S = (float)s, B = (float)b;
            var c = LayerPaint.Color.FromHSV(H, S, B);
            set(c, H, S, B);
        }
        public void setColor(LayerPaint.Color c)
        {
            set(c, c.H, c.S, c.V);
        }

        public void drawPick()
        {
            int slen = (int)(pwb.PixelWidth);
            int len = (int)(pwb.PixelHeight);
            float ps = 1.0f / slen, pl = 1.0f / len;
            LayerPaint.IGrap.fillColor(pwb, (x, y) => {
                return LayerPaint.Color.FromHSV(angle, x * ps, (len - y) * pl);
            });
        }
        public void drawSlide()
        {
            int w = (int)(swb.PixelHeight * 0.5);
            double w2 = (w - (int)Pick.Margin.Bottom * 2); w2 = Math.Sqrt((w2 * w2) * 2);
            Stream stream = swb.PixelBuffer.AsStream();
            int len = (int)(swb.PixelHeight), cx = (int)(len * 0.5);
            int slen = (int)(swb.PixelWidth), cy = (int)(slen * 0.5);
            LayerPaint.IGrap.fillColor(swb, (s, l) => {
                int X = s - cx;
                int Y = l - cy;
                var h = (float)LayerPaint.Vec2.getAngle(X, Y);
                double p = Math.Sqrt(((X * X) + (Y * Y)));
                var C = LayerPaint.Color.FromHSV(h, 1, 1);
                C.A = (byte)(w2 < p && p < w ? 255 : 0);
                return C;
            });
        }
    }
}
