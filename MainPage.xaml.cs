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
using Windows.UI.Xaml.Media.Animation;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Hosting;
using Windows.UI;
using App2.Model;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.ViewManagement;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.Pickers;
using LayerPaint;
using Windows.ApplicationModel.Email;
using Windows.System;
using Windows.UI.Composition;
using System.Numerics;
using App1.View;
using Windows.ApplicationModel.DataTransfer;

namespace App2
{
    class CC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var cc = (Windows.UI.Color)value;
            return string.Format("#{0:X2}{1:X2}{2:X2}", cc.R, cc.G, cc.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    class BB : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    class DD : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(bool)value;
        }
    }
    public sealed partial class MainPage : Page  
    { 
        public static MainPage Current => (Window.Current.Content as Frame).Content as MainPage;
        public Windows.UI.Color MainColor
        {
            get => vm.MainBrush.Color;
            set => vm.MainBrush.Color = value;
        }
        public Windows.UI.Color BackColor => vm.BackBrush.Color;
        public Rect DrawRect => vm.DrawRect;
         

        public MainPage()
        {
            //Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en";//test
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            ((Action)async delegate {
              
                while (true)
                {
                    await Task.Delay(1000);

                    var fn = Path.GetFileName(vm.FileName);
                    DEBUG.Text = (fn.Length>1?fn:"*") + " - " + DRAW.CurrentTouchCount + " " + DRAW.State + " " + Exec.Count + " " + DRAW.Prs;
                    ApplicationView.GetForCurrentView().Title = DEBUG.Text;
                    DB.Maximum = MemoryManager.AppMemoryUsageLimit;
                    DB.Value = MemoryManager.AppMemoryUsage;
                    //DB.Background = new SolidColorBrush(LayerPaint.Color.FromArgb(0, DB.Value / DB.Maximum * 255, 0, 0));

                    if (vm.LayerList.Count > 1)
                    { 
                    // vm.LayerList.Last().Bitmap= await ((FrameworkElement)DRAW.ITEMS.ContainerFromItem(vm.LayerList.First())).Render();
                    }
                }
            })();

            // var b = CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
            // if (b)
            // {
            //     Debug.WriteLine("BUGBUGBUGBUGBUGBUGBUGBUGBUGBUGBUGBUGBUG");
            //     CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false; 
            // }


            //ApplicationView.GetForCurrentView().ExitFullScreenMode();

            //ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default).ToString();
            //ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay).ToString();

            DependencyPropertyChangedCallback a = (s, e) => {
                if ((s as VModel).PanelReverse == true)
                {
                    SL.HorizontalAlignment = HorizontalAlignment.Right;
                    SL.PanePlacement = SplitViewPanePlacement.Right;
                    SR.HorizontalAlignment = HorizontalAlignment.Left;
                    SR.PanePlacement = SplitViewPanePlacement.Left;
                }
                else
                {
                    SL.HorizontalAlignment = HorizontalAlignment.Left;
                    SL.PanePlacement = SplitViewPanePlacement.Left;
                    SR.HorizontalAlignment = HorizontalAlignment.Right;
                    SR.PanePlacement = SplitViewPanePlacement.Right;
                }
            };
            vm.RegisterPropertyChangedCallback(VModel.PanelReverseProperty,a);

            DependencyPropertyChangedCallback b = (s, e) => {
                if ((s as VModel).BarPosition == true)
                {
                    Cmd.VerticalAlignment = VerticalAlignment.Top;
                    SL.Margin = SR.Margin = DRAW.Margin = new Thickness(0, 60, 0, 0);
                    
                }
                else
                {
                    Cmd.VerticalAlignment = VerticalAlignment.Bottom;
                    SL.Margin = SR.Margin = DRAW.Margin = new Thickness(0, 0, 0, 60);
                }
            };
            vm.RegisterPropertyChangedCallback(VModel.BarPositionProperty,b);

            Dispatcher.RunIdleAsync(_ => {
                a(vm, VModel.PanelReverseProperty);
                b(vm, VModel.BarPositionProperty);
            }).AsTask();

            //VerticalAlignment = "{x:Bind vm.BarPosition,Mode=OneWay}"


            var c = new UISettings();
            var color = c.GetColorValue(UIColorType.AccentLight3);
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                DB.Background = Cmd.Background = PIVOT.Background = new AcrylicBrush() { TintColor = color, TintOpacity = 0.4 };
            }
            else
            {
               DB.Background= Cmd.Background = PIVOT.Background =  new SolidColorBrush() { Color = color, Opacity = 0.9 };
            }

            //phone
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = Colors.Black;
            }

            //pc
            var t=ApplicationView.GetForCurrentView().TitleBar;
            t .BackgroundColor = t.ButtonBackgroundColor = c.GetColorValue(UIColorType.Accent);
            t.InactiveBackgroundColor = t.ButtonInactiveBackgroundColor = color;
            


            //CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            var cc = Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;
            var aa = Windows.Globalization.ApplicationLanguages.Languages.ToArray();

            //if(ApplicationView.GetForCurrentView().Orientation == ApplicationViewOrientation.Landscape)
            //{
            //    ApplicationView.GetForCurrentView().TryEnterFullScreenMode();pc不能用
            //}
            Windows.Graphics.Display.DisplayInformation.GetForCurrentView().OrientationChanged += (s, e) => {
                if (s.CurrentOrientation == s.NativeOrientation)
                {
                    ApplicationView.GetForCurrentView().ExitFullScreenMode();
                }
                else
                {
                    ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                }
                DRAW.ResizePanel();
            };
#if !DEBUG
               Pin.Visibility = Visibility.Collapsed;
               Exit.Visibility = Visibility.Collapsed;
#endif
        }



        private void BarLayer_Tapped(object sender, RoutedEventArgs e)
        {
            var p = sender as AppBarButton;
            DRAW.ScaleX = !DRAW.ScaleX;
            p.Label = DRAW.ScaleX ? "off" : "on";
        }


        public async void OnCreate(object sender, RoutedEventArgs e)
        {
            await new LayerPaint.NewFileDialog(vm).ShowAsync();//新画板
        }
        public async void OnSave(object sender, RoutedEventArgs e)
        {
            var W = Convert.ToInt32(vm.DrawRect.Width);
            var H = Convert.ToInt32(vm.DrawRect.Height);
            await vm.SaveFile(await vm.CurrentFile, vm.LayerList, W, H);
            await vm.backup();
        }
        public async void OnImport(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker();
            //openPicker.ContinuationData["new"] = true; 
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add(".psd");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");
            var files = await openPicker.PickMultipleFilesAsync();

            vm.Loading = true;
            List<LayerModel> ls = new List<LayerModel>();
            foreach (var file in files)
            {
                await vm.LoadFile(file, ls);
            }
            if (ls.Count == 0) return;
            ls.Reverse();
            Exec.Do(new Exec() {
                exec = delegate {
                    foreach (var item in ls)
                        vm.LayerList.Insert(0,item);
                    vm.CurrentLayer = ls[0];
                },
                undo = delegate {
                    foreach (var item in ls)
                        vm.LayerList.Remove(item);
                }
            });
            vm.Loading = false;
        }
        public async void OnImportClipboard(object sender, TappedRoutedEventArgs e)
        {
            vm.Loading = true;
            DataPackageView con = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
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
        }
        public async void OnExport(object sender, RoutedEventArgs e)
        {
            var picker = new FileSavePicker(); 
            var d = DateTime.Now;
            picker.SuggestedFileName = string.Format("{0}{1}{2}{3:00}{4:00}{5}", d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);
            picker.FileTypeChoices.Add("psd", new List<string>() { ".psd", ".png" });
            picker.FileTypeChoices.Add("jpg", new List<string>() { ".jpg" });
            picker.FileTypeChoices.Add("gif", new List<string>() { ".gif" });
            picker.FileTypeChoices.Add("png", new List<string>() { ".png" });
            var file = await picker.PickSaveFileAsync();
            if (file == null)
            {
                return;
            }
            var W = Convert.ToInt32(vm.DrawRect.Width);
            var H = Convert.ToInt32(vm.DrawRect.Height);
            await vm.SaveFile(file, vm.LayerList, W, H);
        }
        public void OnSetting(object sender, RoutedEventArgs e)
        { 
            
            Frame.Navigate(typeof(Setting));
        }

        private void BarUndo_Tapped(object sender, RoutedEventArgs e)
        {
            LayerPaint.Exec.Undo();//撤销
        }
        private void BarRedo_Tapped(object sender, RoutedEventArgs e)
        {
            LayerPaint.Exec.Redo();//重做
        }

        private async void OnFeedback(object sender, TappedRoutedEventArgs e)
        {
            var em = new EmailMessage();
            em.Subject = "PrPaint Help";
            em.To.Add(new EmailRecipient("2276225819@qq.com"));
            await EmailManager.ShowComposeNewEmailAsync(em);
        } 
        private async void OnExit(object sender, TappedRoutedEventArgs e)
        {
            await vm.reset();
            //Application.Current.Exit();
        }
 
        private void AppBarButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("M");
            var sss = sender as FrameworkElement;
            var tip = Flyout.GetAttachedFlyout(sss); 
            tip.ShowAt(sss);
        }

        private void AppBarButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("W");
            var sss = sender as FrameworkElement;
            var tip = Flyout.GetAttachedFlyout(sss);
            tip.Hide();
        }

        private void OnPin(object sender, TappedRoutedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();
            var mode = view.ViewMode == ApplicationViewMode.Default ? ApplicationViewMode.CompactOverlay : ApplicationViewMode.Default;
            ApplicationView.GetForCurrentView().TryEnterViewModeAsync(mode).ToString();
        }


        class ByteColor
        {
            byte a, r, g, b;
            byte A { get; set; }
            byte R { get; set; }
            byte G { get; set; }
            byte B { get; set; }
            public void Multiply(ByteColor c)
            {
                r = (byte)Math.Max(0, r + c.r);
                g = (byte)Math.Max(0, g + c.g);
                b = (byte)Math.Max(0, b + c.b);
                a = (byte)Math.Min(255, a + c.a);
            } 
            public void Normal(ByteColor c)
            {

            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
