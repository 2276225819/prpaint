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

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace LayerPaint
{
    public sealed partial class NewFileDialog : ContentDialog
    {


        public string H
        {
            get { return (string)GetValue(HProperty); }
            set { SetValue(HProperty, value); }
        }

        // Using a DependencyProperty as the backing store for H.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HProperty =
            DependencyProperty.Register("H", typeof(string), typeof(NewFileDialog), new PropertyMetadata(""));



        public string W
        {
            get { return (string)GetValue(WProperty); }
            set { SetValue(WProperty, value); }
        }

        // Using a DependencyProperty as the backing store for W.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WProperty =
            DependencyProperty.Register("W", typeof(string), typeof(NewFileDialog), new PropertyMetadata(""));




        public NewFileDialog(VModel d)
        {
            this.InitializeComponent();
            W = d.FileWidth;
            H = d.FileHeight;
            //新建
            First.Click += (s, e) => {
                if (int.TryParse(W, out var w) && int.TryParse(H, out var h) && w > 1 && h > 1)
                {
                    d.FileWidth = W;
                    d.FileHeight = H;


                    d.LayerList.Clear();
                    d.DrawRect = new Rect() { Width = w, Height = h };
                    d.LayerList.Add(new LayerModel() { });
                    d.FileName = "";
                    d.CurrentLayer = d.LayerList[0];
                    Exec.Clean();
                    Hide();
                }
            };
            //打开
            Open.Click += async (s, e) => {
                Hide();
                FileOpenPicker openPicker = new FileOpenPicker();
                //openPicker.ContinuationData["new"] = true; 
                openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

               
                openPicker.ViewMode = PickerViewMode.List;
                openPicker.FileTypeFilter.Add(".psd");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".png");
                var file = await openPicker.PickSingleFileMux();
                if (file == null)
                {
                    return;
                }

                d.LayerList.Clear();
                d.FileName = file.Path;
                await d.LoadFile(file, d.LayerList, (w, h) => {
                    d.DrawRect = new Rect() { Width = w, Height = h };
                });
                d.CurrentLayer = d.LayerList[0];
                Exec.Clean();
            };
            //取消
            Second.Click += (s, e) => {
                base.Hide();
            };
            //重置
            ReSize.Click += (s, e) => {
                W = Math.Floor( Window.Current.Bounds.Width).ToString();
                H = Math.Floor( Window.Current.Bounds.Height).ToString();
            };

        } 
    }
}
