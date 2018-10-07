using App2.Model;
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
using App2.Model.Tools;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;
using LayerPaint;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.Storage;
//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace App2.View
{
    public sealed partial class ToolsAttr : UserControl
    {
        public ToolsAttr()
        {
            this.InitializeComponent();

            Dispatcher.RunIdleAsync((_) => {
                if (VModel.vm == null || ScaleObj==null) return;
                VModel.vm.RegisterPropertyChangedCallback(VModel.CurrentToolsProperty, (s, e) => {
                    SetSelected((ToolsModel)s.GetValue(e));
                });

                SetSelected(VModel.vm.CurrentTools);


                ScaleObj.OnChangeDraw = (s) => {
                    sz_ValueChanged(null, null);
                };
            }).ToString();
        }


        public ToolsModel CurrentTools
        {
            get { return VModel.vm.CurrentTools; }
            set { VModel.vm.CurrentTools = value; }
        }



        public ObservableCollection<ToolsModel> Items { get; set; }


        public ClipModel Clipper { get; set; }

        public DrawPanel ScaleObj { get; set; }



        void SetSelected(ToolsModel e)
        {
            switch (e)
            {
                case ClipModel clip:
                    Attr.Template = ClipAttr;
                    Attr.DataContext = e;
                    break;
                case PenModel pen:
                    Attr.Template = PenAttr;
                    Attr.DataContext = e;
                    break;
                case TextModel text:
                    Attr.Template = TextAttr;
                    Attr.DataContext = e;
                    break;
                case FillModel fill:
                    Attr.Template = FillAttr;
                    Attr.DataContext = e;
                    break;
                default:
                    Attr.Template = null;
                    break;
            }
        }

        private void OnPenAttr1(object sender, TappedRoutedEventArgs e)
        {
            var t = (sender as FrameworkElement).DataContext as ToolsModel;
            Items.Insert(Items.IndexOf(t), ToolsModel.Create(t.ToData()));
        }
        private void OnPenAttr2(object sender, TappedRoutedEventArgs e)
        {
            var t = (sender as FrameworkElement).DataContext as ToolsModel;
            if (Items.Where(x => x.Icon == t.Icon).Count() > 1)
            {
                Items.Remove(t);
            }
        }
        private void OnPenAttr3(object sender, TappedRoutedEventArgs e)
        {
            var t = (sender as FrameworkElement).DataContext as ToolsModel;
            var i = Items.IndexOf(t);
            if (i > 0)
            {
                Items.Move(i, i - 1);
            }
        }
        private void OnPenAttr4(object sender, TappedRoutedEventArgs e)
        {
            var t = (sender as FrameworkElement).DataContext as ToolsModel;
            var i = Items.IndexOf(t);
            if (i < Items.Count - 1)
            {
                Items.Move(i, i + 1);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var t = (sender as FrameworkElement).DataContext as ToolsModel;
            if (t.Name != (sender as TextBox).Text)
            {
                t.Name = (sender as TextBox).Text;
                Items[Items.IndexOf(t)] = t;
            }
        }

        bool loc = false;
        private void sz_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (loc) return;
            loc = true;
            Dispatcher.RunIdleAsync(async x => {


                Debug.WriteLine("ATTR");
                var t = CurrentTools as PenModel;
                if (t != null)
                { 
                    var tmp = t.tmp;
                    var g = new Graphics(Color.Black, tmp);
                    var c = g.Color;
                    float s = t.size * (float)ScaleObj.Scale;
                    c.A = 0;
                    g.Color = c;
                    await g.DrawRect(0, 0, tmp.PixelWidth, tmp.PixelHeight);
                    c.A = 255;
                    g.Color = c;
                    g.Size = s / 2;


                    var a = new Vec2(tmp.PixelWidth * 0.00, tmp.PixelHeight * 1.00);
                    var b = new Vec2(tmp.PixelWidth * 1.00, tmp.PixelHeight * 0.00);
                    var c1 = new Vec2(tmp.PixelWidth * 0.25, tmp.PixelHeight * 0.25);
                    var c2 = new Vec2(tmp.PixelWidth * 0.75, tmp.PixelHeight * 0.75);


                    await g.DrawBerzier(a, c1, c2, s / 2, t.hard, t.opacity, t.density);
                    await g.DrawBerzier(c1, c2, b, s / 2, t.hard, t.opacity, t.density);

                    await Task.Delay(100);
                    g.Invalidate();
                    await Task.Delay(100);
                }
                loc = false;
            }).ToString();
        }
         

        private void OnClipAttrCancel(object sender, TappedRoutedEventArgs e)
        {
            Clipper.Points.Clear();
        }

        private async void OnClipAttrCopy(object sender, TappedRoutedEventArgs e)
        {
            var vm = VModel.vm;
            vm.Loading = true;
            var ot = await Clipper.CopyImage(VModel.vm.CurrentLayer);
            if (ot != null)
            {
                var stream = new InMemoryRandomAccessStream();
                var d = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                d.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight,
                    (uint)ot.PixelWidth, (uint)ot.PixelHeight, 96, 96,
                    ot.PixelBuffer.ToArray());
                await d.FlushAsync();

                var ss = RandomAccessStreamReference.CreateFromStream(stream);
                var dd = new DataPackage();
                dd.SetBitmap(ss);
                Clipboard.SetContent(dd);
            }
            vm.Loading = false;
        }

        private async void OnClipAttrPaste(object sender, TappedRoutedEventArgs e)
        {
            var vm = VModel.vm;
            vm.Loading = true;
            DataPackageView con = Clipboard.GetContent();
            if (con.Contains(StandardDataFormats.Bitmap))
            {
                var img = await con.GetBitmapAsync();
                WriteableBitmap src = await Img.CreateAsync(img);
                Exec.Do(new Exec() {
                    exec = delegate {
                        vm.LayerList.Insert(0, new LayerModel() {
                            Bitmap = src
                        });
                        vm.CurrentLayer = vm.LayerList[0];
                    },
                    undo = delegate {
                        vm.LayerList.RemoveAt(0);
                    }
                });
            }
            else
            {
                await Task.Delay(500);
            }
            vm.Loading = false;
            MainPage.Current.PivotIndex = 2;
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
           //var v = new ResizeModel();
           //v.OnBegin(VModel.vm.CurrentLayer);
           //VModel.vm.CurrentTools = v;
           //var b = VModel.vm.CurrentTools;


        } 
    }
}
