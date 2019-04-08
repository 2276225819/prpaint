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
using Windows.UI.Popups;
using Windows.Storage.Pickers;
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


                VModel.vm.RegisterPropertyChangedCallback(VModel.CurrentLayerProperty, async (s, e) => {
                    await Task.Yield();
                    SetSelected(VModel.vm.CurrentTools);
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



        public void SetSelected(ToolsModel e)
        {
            Attr.Template = null;
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
                case Pen9Model pen9:
                    Attr.Template = Pen9Attr;
                    Attr.DataContext = e;
                    break;
                case TxEditModel text:
                    Attr.Template = TextAttr;
                    Attr.DataContext = e;
                    break;
                case FillModel fill:
                    Attr.Template = FillAttr;
                    Attr.DataContext = e;
                    break;
                case PickerModel pick:
                case BuckerModel buck:
                    Attr.Template = PickAttr;
                    Attr.DataContext = e;
                    break;
                default:
                    break;
            }
        }

        public T Elem<T>(Action<T> cb) where T : DependencyObject, new()
        {
            var tcss = new T();
            cb(tcss);
            return tcss;
        }

        private void OnPenAttr1(object sender, TappedRoutedEventArgs e)
        {
            var a = new MenuFlyout();
            a.Items.Add(Elem<MenuFlyoutItem>(x => {
                x.Text = "pen(old)";
                x.Click += (ss, ee) => {
                    var t = (sender as FrameworkElement).DataContext as ToolsModel;
                    Items.Insert(Math.Max(0, Items.IndexOf(t)), new PenModel { Name = "pen" });
                };
            }));
            a.Items.Add(Elem<MenuFlyoutItem>(x => {
                x.Text = "pen";
                x.Click += (ss, ee) => {
                    var t = (sender as FrameworkElement).DataContext as ToolsModel;
                    Items.Insert(Math.Max(0, Items.IndexOf(t)), new Pen9Model { Name = "pencil" });
                };
            }));
            a.Items.Add(Elem<MenuFlyoutItem>(x => {
                x.Text = "spread";
                x.Click += (ss, ee) => {
                    var t = (sender as FrameworkElement).DataContext as ToolsModel;
                    Items.Insert(Math.Max(0, Items.IndexOf(t)), new Pen9Model {
                        Name = "spread",
                        size = 24,
                        size_fade = false,
                        size_prs = false,
                        density_prs = true,
                        density = 0.3f,
                        bgurl = "ms-appx:///Assets/Texture/bb.png",
                    });
                };
            }));
            a.Items.Add(Elem<MenuFlyoutItem>(x => {
                x.Text = "bristle";
                x.Click += (ss, ee) => {
                    var t = (sender as FrameworkElement).DataContext as ToolsModel;
                    Items.Insert(Math.Max(0, Items.IndexOf(t)), new Pen9Model {
                        Name = "bristle",
                        size = 16,
                        randsize = 5,
                        randrota = 2,
                        space = 0.15f,
                        size_fade = false,
                        fgurl = "ms-appx:///Assets/Texture/bb.png",
                    });
                };
            }));
            a.Items.Add(Elem<MenuFlyoutItem>(x => {
                x.Text = "water color";
                x.Click += (ss, ee) => {
                    var t = (sender as FrameworkElement).DataContext as ToolsModel;
                    Items.Insert(Math.Max(0, Items.IndexOf(t)), new Pen9Model {
                        Name = "bristle",
                        size_fade=false,
                        size_prs=false,
                        density_prs=true, 
                        size = 24,
                        blend = 1,
                        dilution = 0.8f, 
                    });
                };
            })); 
            a.Items.Add(Elem<MenuFlyoutItem>(x => {
                x.Text = "eraser";
                x.Click += (ss, ee) => {
                    var t = (sender as FrameworkElement).DataContext as ToolsModel;
                    Items.Insert(Math.Max(0, Items.IndexOf(t)), new EraserModel { });
                };
            }));

            a.ShowAt(sender as FrameworkElement); 
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
        WriteableBitmap tmp = new WriteableBitmap(200, 100);
        private void sz_ValueChanged(object sender, object e)
        {
            if (loc) return;
            loc = true;
            Dispatcher.RunIdleAsync(async x => { 
                Debug.WriteLine("ATTR"); 
                if (CurrentTools is PenModel t)
                {
                    var g = new Graphics(Color.Black, tmp);
                    var c = g.Color;
                    float s = t.size / 4 * (float)ScaleObj.Scale;
                    c.A = 0;
                    g.Color = c;
                    await g.DrawRect(0, 0, tmp.PixelWidth, tmp.PixelHeight);
                    c.A = 255;
                    g.Color = c;
                    g.Size = s ;


                    var a = new Vec2(tmp.PixelWidth * 0.00, tmp.PixelHeight * 1.00);
                    var b = new Vec2(tmp.PixelWidth * 1.00, tmp.PixelHeight * 0.00);
                    var c1 = new Vec2(tmp.PixelWidth * 0.25, tmp.PixelHeight * 0.25);
                    var c2 = new Vec2(tmp.PixelWidth * 0.75, tmp.PixelHeight * 0.75);


                    await g.DrawBerzier(a, c1, c2, s / 2, t.hard, 1, t.density);
                    await g.DrawBerzier(c1, c2, b, s / 2, t.hard, 1, t.density);

                    await Task.Delay(100);
                    g.Invalidate();
                    await Task.Delay(100);
                }
                if(CurrentTools is Pen9Model tt)
                {
                    Pen9Model.gdi.Init(tmp, tt);
                    Pen9Model.gdi.step = 1;
                    Pen9Model.gdi.size = tt.size / 2 * (float)ScaleObj.Scale;
                    Pen9Model.gdi.Clear(Windows.UI.Colors.Transparent);
                    Pen9Model.gdi.MoveTo((float)(tmp.PixelWidth * 0.00), (float)(tmp.PixelHeight * 1.00), 1);
                    Pen9Model.gdi.LineTo((float)(tmp.PixelWidth * 0.25),(float)( tmp.PixelHeight * 0.25), 1);
                    Pen9Model.gdi.LineTo((float)(tmp.PixelWidth * 0.75),(float)( tmp.PixelHeight * 0.75), 1);
                    Pen9Model.gdi.DrawBerzier();
                    Pen9Model.gdi.LineTo((float)(tmp.PixelWidth * 1.00),(float)( tmp.PixelHeight * 0.00), 1); 
                    Pen9Model.gdi.DrawBerzier();
                    Pen9Model.gdi.DrawEnd(true);
                    Pen9Model.gdi.Invalidate();

                    if (fg != null) fg.Source = LayerPaint.Img.Create(tt.fgurl);
                    if (bg != null) bg.Source = LayerPaint.Img.Create(tt.bgurl);
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
            await VModel.vm.Copy();
        }
        private async void OnClipAttrSS(object sender, TappedRoutedEventArgs e)
        {
            await VModel.vm.PastsSS();
            MainPage.Current.PivotIndex = 2;
        }
        private async void OnClipAttrPaste(object sender, TappedRoutedEventArgs e)
        {
            await VModel.vm.Pasts();
            MainPage.Current.PivotIndex = 2;
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
           //var v = new ResizeModel();
           //v.OnBegin(VModel.vm.CurrentLayer);
           //VModel.vm.CurrentTools = v;
           //var b = VModel.vm.CurrentTools;


        }
          
        private void TextBox_LostFocus(object sender, object e)
        {
            ((sender as FrameworkElement)?.DataContext as TxEditModel)?.OnReflush(true);
        }
         
        private void FontListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if((sender as FrameworkElement).Parent != null)
            {
                ((sender as FrameworkElement)?.DataContext as TxEditModel)?.OnReflush(true);
            } 
        }
        int bb = 0;
        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if ((sender as FrameworkElement).Parent != null)
            {
                ((sender as FrameworkElement)?.DataContext as TxEditModel)?.OnReflush(false);

                if (bb++ == 0) ((Action)async delegate {
                    while (true)
                    { 
                        await Task.Delay(300);
                        if (bb == 1)
                        {
                            Focus(FocusState.Pointer);
                            bb = 0;
                            return;
                        }
                        bb = 1;
                    }
                })();
            }

        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            var img = sender as Image;
            img.Source = tmp; 
        }

        private void ToggleSwitch_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var ss = sender as PickerModel;
            ss.OnToolState(DrawPanel.Current, false);
        }

        private async void MenuFlyoutItem_Click_t(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement).DataContext is Pen9Model pen9)
            { 
                var openPicker = new FileOpenPicker();
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".png");
                openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                openPicker.ViewMode = PickerViewMode.Thumbnail;
                try
                {
                    var res = await openPicker.PickSingleFileMux();
                    if (res != null)
                    {
                        Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(res);

                        pen9.bgurl = res.Path;
                        sz_ValueChanged(null, null); 
                    }
                }
                catch (Exception)
                { 
                }
            }
        }

        private async void MenuFlyoutItem_Click_s(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement).DataContext is Pen9Model pen9)
            { 
                var openPicker = new FileOpenPicker();
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".png");
                openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                openPicker.ViewMode = PickerViewMode.Thumbnail;
                try
                {
                    var res = await openPicker.PickSingleFileMux();
                    if (res != null)
                    {
                        Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(res);

                        pen9.fgurl = res.Path;
                        sz_ValueChanged(null, null);
                    }
                }
                catch (Exception)
                { 
                }
            }
        }

        private void Button_Click_t(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement).DataContext is Pen9Model pen9)
            {
                pen9.bgurl = "";
                sz_ValueChanged(null, null);
            }
        }

        private void Button_Click_s(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement).DataContext is Pen9Model pen9)
            {
                pen9.fgurl = "";
                sz_ValueChanged(null, null);
            }
        }
         
        Image fg, bg;
        private void Image_Loaded_t(object sender, object e)
        {
            bg = sender as Image;
            sz_ValueChanged(null, null);
        }

        private void Image_Loaded_s(object sender, object e)
        {
            fg = sender as Image; 
            sz_ValueChanged(null, null);
        }
    }
}
