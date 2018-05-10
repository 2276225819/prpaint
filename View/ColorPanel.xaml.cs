using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace App2.View
{
    public sealed partial class ColorPanel : UserControl
    {
        public SolidColorBrush MainBrush { get; set; } 
        bool loc;
        public double R
        {
            get { return Convert.ToDouble(GetValue(RProperty)); }
            set { loc = true; SetValue(RProperty, value); MainBrush.Color = (new Color() { R = (byte)value, G = (byte)G, B = (byte)B, A = 255 }); loc = false; }
        }

        // Using a DependencyProperty as the backing store for R.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RProperty =
            DependencyProperty.Register("R", typeof(double), typeof(ColorPanel), new PropertyMetadata(0));



        public double G
        {
            get { return Convert.ToDouble(GetValue(GProperty)); }
            set { loc = true; SetValue(GProperty, value); MainBrush.Color = (new Color() { R = (byte)R, G = (byte)value, B = (byte)B, A = 255 }); loc = false; }
        }

        // Using a DependencyProperty as the backing store for G.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GProperty =
            DependencyProperty.Register("G", typeof(double), typeof(ColorPanel), new PropertyMetadata(0));



        public double B
        {
            get { return Convert.ToDouble(GetValue(BProperty)); }
            set { loc = true; SetValue(BProperty, value); MainBrush.Color = (new Color() { R = (byte)R, G = (byte)G, B = (byte)value, A = 255 }); loc = false; }
        }

        // Using a DependencyProperty as the backing store for B.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BProperty =
            DependencyProperty.Register("B", typeof(double), typeof(ColorPanel), new PropertyMetadata(0));

 


        public ColorPanel()
        {
            this.InitializeComponent();
            Dispatcher.RunIdleAsync((s) => {
                if (MainBrush == null) return;
                var c = MainBrush.Color;
                TXT.Text = string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);

                SetValue(RProperty, c.R);
                SetValue(GProperty, c.G);
                SetValue(BProperty, c.B);
                MainBrush.RegisterPropertyChangedCallback(SolidColorBrush.ColorProperty, (obj, prop) => {
                    Color cc = (Color)obj.GetValue(prop);
                    TXT.Text = string.Format("#{0:X2}{1:X2}{2:X2}", cc.R, cc.G, cc.B);
                    //((SolidColorBrush)TXT.Foreground).Color = Color.FromArgb(255,(byte)( 255 - cc.R),(byte)( 255 - cc.G),(byte)( 255 - cc.B));
                    if (!loc)
                    {
                        SetValue(RProperty, cc.R);
                        SetValue(GProperty, cc.G);
                        SetValue(BProperty, cc.B);
                    }
                });
            }).ToString();
        } 
    }
}
