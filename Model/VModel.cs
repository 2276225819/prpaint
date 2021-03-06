﻿using App2.Model;
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
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
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
        Encoding encoding;
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
                ToolsList.Add(new Model.Tools.Pen9Model() { Name = "pen1", });
                ToolsList.Add(new Model.Tools.EraserModel() { Name = "eras", });
            }
            if (ToolsList.Count > 0)
            {
                CurrentTools = ToolsList[1];
                CurrentTools = ToolsList[0];
            }
            if(Data["Error"] is string ee){
                Data.Remove("Error");
                var em = new EmailMessage();
                em.Subject = "Prpaint Crash Report";
                em.Body = ee;
                em.To.Add(new EmailRecipient("2276225819@qq.com"));
                EmailManager.ShowComposeNewEmailAsync(em).ToString();
            }

            Window.Current.VisibilityChanged += (s, e) => {
                if (e.Visible) return;
                Window.Current.Activate();
                backup();
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

            App.Current.UnhandledException += (sender, e) => {
                Data["Error"] = e.Exception.ToString();
                vm.backup();
            };
        }
        public async void act()
        {
            encoding = await GetLocalEncode();
            //新窗口? 打开tmp 
            if (vm.LayerList.Count == 0)
            {
                try
                {
                    Debug.WriteLine("------------open tmp----------------");
                    var dir = await KnownFolders.PicturesLibrary.CreateFolderAsync("Prpaint", CreationCollisionOption.OpenIfExists);
                    StorageFile tmp = await dir.GetFileAsync("_tmp");
                    await vm.LoadFile(tmp, vm.LayerList, (x, y) => {
                        vm.DrawRect = new Rect() { Width = x, Height = y };
                        Exec.Clean();
                    });
                }
                catch (Exception)
                {
                    if (FileName != "")
                    {
                        vfn = FileName; //tmp打开失败 尝试打开历史文件
                    }
                }
            }
            //打开新图片需要否需要保存tmp? 保存
            if (vfn != null && vm.LayerList.Count != 0)
            {
                try
                { 
                    // // 手机不支持
                    // var messageDialog = new MessageDialog(LANG("_save_old_file"),"Prpaint");
                    // messageDialog.Commands.Add(new UICommand(LANG("_yes"), x=> { }, 1));
                    // messageDialog.Commands.Add(new UICommand(LANG("_no"), x => { } ,2));
                    // //messageDialog.Commands.Add(new UICommand(LANG("_cancel"), x => { }, 3));
                    // messageDialog.DefaultCommandIndex = 1;
                    // var res = await messageDialog.ShowMux();
                    // switch (res.Id)
                    // {
                    //     case 1:
                    //         var fname = await CurrentFile;
                    //         await SaveFile(fname, LayerList, (int)DrawRect.Width, (int)DrawRect.Height);
                    //         break;
                    //     case 2:
                    //         break;
                    //     default:
                    //         vfn = null;
                    //         break;
                    // }

                    var md = new ContentDialog();
                    md.Title = LANG("_save_old_file");
                    md.Content = Elem<StackPanel>(s => {
                        s.Orientation = Orientation.Horizontal;
                        s.HorizontalAlignment = HorizontalAlignment.Right;
                        s.Children.Add(Elem<Button>(b => {
                            b.Content = LANG("_yes");
                            b.Click += async (ss,ee) => {
                                var fname = await CurrentFile;
                                await SaveFile(fname, LayerList, (int)DrawRect.Width, (int)DrawRect.Height);
                                md.Hide();
                            };
                        }));
                        s.Children.Add(Elem<Button>(b => {
                            b.Content = LANG("_no");
                            b.Click += (ss, ee) => {
                                md.Hide();
                            };
                        }));
                        s.Children.Add(Elem<Button>(b => {
                            b.Content = LANG("_cancel");
                            b.Click += (ss, ee) => {
                                vfn = null;
                                md.Hide();
                            };
                        }));
                    });
                    await md.ShowMux();
                    T Elem<T>(Action<T> cb) where T : DependencyObject, new()
                    {
                        var tcss = new T();
                        cb(tcss);
                        switch (tcss as FrameworkElement)
                        {
                            case Button btn:
                                btn.Margin = new Thickness(5, 0, 5, 0);
                                btn.Padding = new Thickness(15, 5, 15, 5);
                                break; 
                        }
                        return tcss;
                    }
                }
                catch (Exception e)
                {
                    _ = new MessageDialog(e.ToString()).ShowMux();
                }
            }
            //需要打开图片? 继续打开
            if (vfn != null)
            { 
                try
                {
                    StorageFile tmp; 
                    var uri = new Uri(vfn, UriKind.RelativeOrAbsolute);
                    if (uri.IsAbsoluteUri)
                    {
                        tmp = await StorageFile.GetFileFromPathAsync(vfn);
                        Debug.WriteLine("------------open abs----------------");
                    }
                    else
                    {
                        var dir = await KnownFolders.PicturesLibrary.CreateFolderAsync("Prpaint", CreationCollisionOption.OpenIfExists);
                        tmp = await dir.GetFileAsync(vfn);
                        Debug.WriteLine("------------open rel----------------");
                    }
                    await vm.LoadFile(tmp, vm.LayerList, (x, y) => {
                        vm.DrawRect = new Rect() { Width = x, Height = y };
                        vm.LayerList.Clear();
                        vm.FileName = vfn;
                        Exec.Clean();
                    });
                }
                catch (Exception e)
                {
                    _ = new MessageDialog(e.ToString()).ShowMux();
                }
            }
            if (vm.LayerList.Count == 0)
            {
                vm.LayerList.Add(new LayerModel() { });
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

        public void backup()
        {
            try
            {
                Debug.WriteLine("BeginBackUp");
                var dir = KnownFolders.PicturesLibrary.CreateFolderAsync("Prpaint", CreationCollisionOption.OpenIfExists).AsTask().GetAwaiter().GetResult();
                var tmp = dir.CreateFileAsync("_tmp", CreationCollisionOption.OpenIfExists).AsTask().GetAwaiter().GetResult();
                using (var s = tmp.OpenStreamForWriteAsync().GetAwaiter().GetResult())
                    SavePSD(s, LayerList, (int)DrawRect.Width, (int)DrawRect.Height);
                Debug.WriteLine("EndBackUp");
            }
            catch (Exception e)
            {
                Data["Error"] = e.ToString();
                new MessageDialog(e.ToString()).ShowMux();
            }
        }
        //public async void checkLog(StorageFile file)
        //{ 
        //    var attr = await file.GetBasicPropertiesAsync();
        //    var mb = attr.Size / 1024 / 1024;
        //    if (mb < 100)  return;
        //
        //    var fmb = mb / 50 * 50;
        //    var logger = Microsoft.Services.Store.Engagement.StoreServicesCustomEventLogger.GetDefault();
        //    logger.Log($"Open{fmb}MB");
        //}
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
                new MessageDialog(e.ToString()).ShowMux();
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
                            using (var s = await file.OpenStreamForWriteAsync()) SavePSD(s, ls, x, y);
                        }
                        break;
                    case ".psd":
                        using (var s = await file.OpenStreamForWriteAsync()) SavePSD(s, ls, x, y);
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
                new MessageDialog(e.ToString()).ShowMux();
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
                var psd = new ConsoleApp1.Mpsd2() { encode = encoding };
                psd.load(stream);
                d?.Invoke(psd.head.width, psd.head.height);
                foreach (var layer in psd.layerdata)
                {
                    int w = layer.Width, h = layer.Height;
                    WriteableBitmap b = null;
                    if (w > 0 && h > 0)
                    {
                        b = new WriteableBitmap(w, h);
                        var bt = layer.getBGRA(w, h);
                        b.PixelBuffer.AsStream().Write(bt, 0, bt.Length);
                    }
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
            }
        }

        public void SavePSD(Stream stream, IList<LayerModel> ls, int w, int h)
        {
            var psd = new ConsoleApp1.Mpsd2() { encode = encoding};
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
            var ttt = Task.WhenAll(tt).GetAwaiter().GetResult();
            for (int i = 0; i < ttt.Length; i++)
            { 
                psd.layerdata.Insert(0, ttt[i]);
            }
            stream.SetLength(0);
            psd.save(stream); 
        }
        public async Task Copy()
        {
            vm.Loading = true;
            var ot = await Clipper.CopyImage(VModel.vm.CurrentLayer);
            if (ot != null)
            {
                var stream = new InMemoryRandomAccessStream();
                var d = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                d.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
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
        public async Task Pasts()
        {
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
                        vm.CurrentLayer = vm.LayerList[0];
                    }
                });
            }
            else
            {
                await Task.Delay(500);
            }
            vm.Loading = false; 
        }
        public async Task PastsSS()
        { 
            vm.Loading = true;
            var ls = new List<LayerModel>();
            try
            {
                var dd = await KnownFolders.PicturesLibrary.GetFolderAsync("Screenshots");
                var ff = await dd.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByDate);
                await vm.LoadFile(ff[0], ls);
            }
            catch (Exception)
            {
                new MessageDialog("nOimaGe").ShowMux();
            }
            vm.Loading = false;
            if (ls.Count != 1)
            {
                return;
            }

            Exec.Do(new Exec() {
                exec = delegate {
                    vm.LayerList.Insert(0, ls[0]);
                    vm.CurrentLayer = vm.LayerList[0];
                },
                undo = delegate {
                    vm.LayerList.RemoveAt(0);
                    vm.CurrentLayer = vm.LayerList[0];
                }
            });
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