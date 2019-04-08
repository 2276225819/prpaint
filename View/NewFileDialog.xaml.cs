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
        public string W { get; set; }
        public string H { get; set; }


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


                Hide();
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
                W = Window.Current.Bounds.Width.ToString();
                H = Window.Current.Bounds.Height.ToString();
            };

        } 
    }
}
