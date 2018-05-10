using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Popups;
using Windows.Storage.Pickers;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using App2.View;
using App2.Model;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using App2;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace LayerPaint
{
    public sealed partial class ColorDialog : ContentDialog
    {
        public SolidColorBrush MainBrush { get; set; }
        public SolidColorBrush BackBrush { get; set; }

        public ColorDialog(VModel d)
        {
            this.InitializeComponent();
            MainBrush = new SolidColorBrush(d.MainBrush.Color);
            
            //新建
            First.Click += (s, e) => {
                d.MainBrush.Color = MainBrush.Color;
                base.Hide();
            }; 
            //取消
            Second.Click += (s, e) => {
                base.Hide();
            }; 

        } 
    }
}
