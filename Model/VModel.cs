using App2.Model;
using App2.Model.Tools;
using App2.View;
using LayerPaint;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace App2.Model
{
    public class VModel : BaseModel
    {
        public static string vfn;
        public static VModel vm;
        public VModel()
        {
            vm = this;
            vm.DrawRect = new Rect() { Width = 800, Height = 800 };
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            act();
            
            if (Data["ColorList"] is string[] cs)
            {
                foreach (var item in cs)
                {
                    ColorList.Add(new SolidColorBrush(Color.FromData(item)));
                }
            }
            if (ColorList.Count < 1)
            { 
                foreach(var i in App1.Vendor.ColorList.List())
                {
                    ColorList.Add(new SolidColorBrush { Color = Color.FromData(i) }); 
                } 
            }
            if ((Data["MainColor"] is string a) && (Data["BackColor"] is string b))
            {
                MainBrush.Color = Color.FromData(a);
                BackBrush.Color = Color.FromData(b);
            }

            if (Data["ToolsList"] is string[] ts)
            {
                foreach (var s in ts)
                {
                    var t = ToolsModel.Create(s);
                    if (t != null) ToolsList.Add(t);
                }
            }
            if (ToolsList.Count < 2)
            {
                ToolsList.Add(new Model.Tools.PenModel() { Name = "pen1", });
                ToolsList.Add(new Model.Tools.EraserModel() { Name = "eras", });
            }
            if (ToolsList.Count > 0)
            {
                CurrentTools = ToolsList[1];
                CurrentTools = ToolsList[0];
            }


            Window.Current.VisibilityChanged += async (s, e) => {
                Window.Current.Activate();
                if (e.Visible) return;
                await backup();
                var c = ColorList.Select(x => {
                    return Color.ToData(x.Color);
                });
                if (c.Count() > 1)
                {
                    Data["ColorList"] = c.ToArray();
                }
                var t = ToolsList.Select(x => {
                    return x.ToData();
                });
                if (t.Count() > 1)
                {
                    Data["ToolsList"] = t.ToArray();
                }

                Data["MainColor"] = Color.ToData(MainBrush.Color);
                Data["BackColor"] = Color.ToData(BackBrush.Color);
            };


        }
        public async void act()
        {
            StorageFile tmp = null;

            if (vfn != null)
            {
                try
                {
                    vm.FileName = vfn;
                    var uri = new Uri(vm.FileName, UriKind.RelativeOrAbsolute);
                    if (uri.IsAbsoluteUri)
                    {
                        tmp = await StorageFile.GetFileFromPathAsync(vm.FileName);
                        Debug.WriteLine("------------open abs----------------");
                    }
                    else
                    {
                        var dir = await KnownFolders.PicturesLibrary.CreateFolderAsync("Prpaint", CreationCollisionOption.OpenIfExists);
                        tmp = await dir.GetFileAsync(vm.FileName);
                        Debug.WriteLine("------------open rel----------------");
                    }
                }
                catch (Exception e)
                {
                    (new MessageDialog(e.ToString())).ShowAsync().ToString();
                    vm.LayerList.Add(new LayerModel() { });
                    FileName = "";
                }
            }
            else
            { 
                try
                {
                    var dir = await KnownFolders.PicturesLibrary.CreateFolderAsync("Prpaint", CreationCollisionOption.OpenIfExists);
                    tmp = await dir.GetFileAsync("_tmp");
                    Debug.WriteLine("------------open tmp----------------");
                }
                catch (Exception)
                {
                    vm.LayerList.Add(new LayerModel() { });
                    FileName = "";
                }
            }
            if (tmp != null)
            {
                vm.LayerList.Clear();
                Exec.Clean();
                await vm.LoadFile(tmp, vm.LayerList, (x, y) => {
                    vm.DrawRect = new Rect() { Width = x, Height = y };
                });
                if (vm.LayerList.Count == 0)
                {
                    vm.LayerList.Add(new LayerModel() { });
                }
                // vm.LayerList.Add(new LayerModel() {
                //     Bitmap = LayerPaint.Img.Create("ms-appx:///Assets/dh.jpg"),
                //     X = -100,
                //     Y = 300,
                // });
            }
            vm.CurrentLayer = vm.LayerList[0];
        }

        public void OnChangeTools(object sender, RoutedEventArgs args)
        {
            CurrentTools = LastTools;
        }
        public void OnChangeColor(object sender, TappedRoutedEventArgs args)
        {
            var c = MainBrush.Color;
            MainBrush.Color = BackBrush.Color;
            BackBrush.Color = c;
        }

        public async Task backup()
        {
            if (Loading == true) return;
            Loading = true;
            try
            {
                Debug.WriteLine("BeginBackUp");
                var dir = await KnownFolders.PicturesLibrary.CreateFolderAsync("Prpaint", CreationCollisionOption.OpenIfExists);
                var tmp = await dir.CreateFileAsync("_tmp", CreationCollisionOption.OpenIfExists);
                await SavePSD(tmp, LayerList, (int)DrawRect.Width, (int)DrawRect.Height);
                Debug.WriteLine("EndBackUp");
            }
            catch (Exception e)
            {
                (new MessageDialog(e.ToString())).ShowAsync().ToString();
            }

            Loading = false;
        }

        public async Task LoadFile(StorageFile file, IList<LayerModel> ls, Action<int, int> d = null)
        {
            await Dispatcher.RunIdleAsync(_ => { }); 
            Loading = true; 
            if (file.FileType == ".png")
            {
                try
                {
                    file = await StorageFile.GetFileFromPathAsync(file.Path.Replace(".png", ".psd"));
                }
                catch (Exception)
                {
                }
            }
            try
            {
                switch (file.FileType.ToLower())
                {
                    case ".":
                    case ".psd":
                        await LoadPSD(file, ls, d);
                        break;
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                    case ".gif":
                        await LoadIMG(file, ls, d);
                        break;
                    default:
                        throw new Exception("ntype");
                }
            }
            catch (Exception e)
            {
                (new MessageDialog(e.ToString())).ShowAsync().ToString();
            }
            if (ls.Count == 0)
            {
                ls.Add(new LayerModel() { });
            }
            Loading = false;
        }


        public async Task SaveFile(StorageFile file, IList<LayerModel> ls, int x, int y)
        {
            await Dispatcher.RunIdleAsync(_ => { });
            Loading = true;
            StorageFolder f = null;
            try
            {
                f = await StorageFolder.GetFolderFromPathAsync(file.Path.Replace(file.Name, "")); 
            }
            catch (Exception)
            {
            }
            try
            {
                WriteableBitmap ot = new WriteableBitmap(x, y);
                foreach (var item in ls.Reverse())
                {
                    if (!item.IsShow) continue;
                    IGrap.addImg(item.Bitmap, ot, (int)item.X, (int)item.Y, item.Opacity);
                }
                switch (file.FileType.ToLower())
                {
                    case ".jpg":
                        await SaveIMG(ot, file, BitmapEncoder.JpegEncoderId);
                        break;
                    case ".gif":
                        await SaveIMG(ot, file, BitmapEncoder.GifEncoderId);
                        return; //xuy 
                    case ".png":
                        await SaveIMG(ot, file, BitmapEncoder.PngEncoderId);
                        if (f != null)
                        {
                            file = await f.CreateFileAsync(file.Name.Replace(".png", ".psd"), CreationCollisionOption.OpenIfExists);
                            await SavePSD(file, ls, x, y);
                        }
                        break;
                    case ".psd":
                        await SavePSD(file, ls, x, y); 
                        if (f != null)
                        {
                            file = await f.CreateFileAsync(file.Name.Replace(".psd", ".png"), CreationCollisionOption.OpenIfExists);
                            await SaveIMG(ot, file, BitmapEncoder.PngEncoderId);
                        }
                        break;
                    default:
                        throw new Exception("ntype");
                }
            }
            catch (Exception e)
            {
                (new MessageDialog(e.ToString())).ShowAsync().ToString();
            }

            Loading = false;
        }
        public async Task LoadIMG(StorageFile file, IList<LayerModel> ls, Action<int, int> d = null)
        {
            var bmp = await Img.CreateAsync(file);
            d?.Invoke(bmp.PixelWidth, bmp.PixelHeight);
            ls.Add(new LayerModel() {
                Bitmap = bmp
            });
        }

        public async Task SaveIMG(WriteableBitmap ot, StorageFile file, Guid type)
        {
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                stream.SetLength(0);
                var d = await BitmapEncoder.CreateAsync(type, stream.AsRandomAccessStream());
                d.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                    (uint)ot.PixelWidth, (uint)ot.PixelHeight, 96, 96,
                    ot.PixelBuffer.ToArray());
                await d.FlushAsync();
            }
        }

        public async Task LoadPSD(StorageFile file, IList<LayerModel> ls, Action<int, int> d = null)
        {
            using (var stream = await file.OpenStreamForReadAsync())
            {
                var psd = new ConsoleApp1.Mpsd2() { encode = await GetLocalEncode() };
                psd.load(stream);
                d?.Invoke(psd.head.width, psd.head.height);
                foreach (var layer in psd.layerdata)
                {
                    int w = layer.Width, h = layer.Height;
                    if (w > 0 && h > 0)
                    {
                        var b = new WriteableBitmap(w, h);
                        var bt = layer.getBGRA(w, h);
                        b.PixelBuffer.AsStream().Write(bt, 0, bt.Length);
                        ls.Insert(0, new LayerModel() {
                            Name = layer.Name,
                            IsShow = layer.Visable,
                            IsEdit = layer.Editable,
                            Opacity = layer.Alpha / 255d,
                            X = (double)layer.Left,
                            Y = (double)layer.Top,
                            Bitmap = b
                        });
                    }
                    else
                    {
                        ls.Insert(0, new LayerModel() {
                            Name = layer.Name,
                            IsShow = layer.Visable,
                            Opacity = layer.Alpha / 255d, 
                        });
                    }
                }
            }
        }

        public async Task SavePSD(StorageFile file, IList<LayerModel> ls, int w, int h)
        {
            var psd = new ConsoleApp1.Mpsd2() { encode = await GetLocalEncode()};
            psd.head = ConsoleApp1.Mpsd2.Head.Create(w, h);
            var tt = new Task<ConsoleApp1.Mpsd2.Layer>[ls.Count];
            for (int i = 0; i < ls.Count; i++)
            {
                var item = ls[i];
                if (item.Bitmap == null)
                {
                    tt[i] = ConsoleApp1.Mpsd2.Layer.CreateAsync(item.Name, (int)item.X, (int)item.Y,
                            0, 0, (byte)(item.Opacity * 255d), item.IsShow,item.IsEdit, new MemoryStream());
                }
                else
                {
                    var b = item.Bitmap;
                    var sm = b.PixelBuffer.AsStream();//Handled = The function evaluation was disabled because of an out of memory exception.
                    if (item.X == Rect.Empty.X)
                    {
                        throw null;
                    }
                    if (item.Name.Length > 255) item.Name = item.Name.Substring(0, 255);
                    //if (item.Name.Length == 0) item.Name = ".";
                    tt[i] = ConsoleApp1.Mpsd2.Layer.CreateAsync(item.Name, (int)item.X, (int)item.Y,
                            b.PixelWidth, b.PixelHeight, (byte)(item.Opacity * 255d), item.IsShow, item.IsEdit, sm);
                }
            }
            var ttt = await Task.WhenAll(tt);
            for (int i = 0; i < ttt.Length; i++)
            { 
                psd.layerdata.Insert(0, ttt[i]);
            }
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                stream.SetLength(0);
                psd.save(stream);
            }
        }

        public async Task reset()
        {
            Data.Clear();

            var dir = await KnownFolders.PicturesLibrary.CreateFolderAsync("Prpaint", CreationCollisionOption.OpenIfExists);
            var tmp = await dir.GetFileAsync("_tmp");
            await tmp.DeleteAsync();
        }

        public async Task<Encoding> GetLocalEncode()
        {
            try
            {
                var cname = await (new WebView()).InvokeScriptAsync("eval", new[] { "document.charset" });
                return Encoding.GetEncoding(cname);
            }
            catch (Exception)
            {
                return Encoding.ASCII;
            }
        }


    }
}