﻿using System;
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
using Windows.UI.Popups;

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
        public static MainPage Current;// => (Window.Current.Content as Frame).Content as MainPage;
        public static void flushtoolattr()
        {
            Current.DF.SetSelected(VModel.vm.CurrentTools);
        }
        public Windows.UI.Color MainColor
        {
            get => vm.MainBrush.Color;
            set => vm.MainBrush.Color = value;
        }
        public Windows.UI.Color BackColor => vm.BackBrush.Color;
        public Rect DrawRect => vm.DrawRect;
        public int PivotIndex
        {
            get => PIVOT.SelectedIndex;
            set => PIVOT.SelectedIndex = value;
        }


        public bool IsHit
        {
            get { return (bool)GetValue(IsHitProperty); }
            set { SetValue(IsHitProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsHit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHitProperty =
            DependencyProperty.Register("IsHit", typeof(bool), typeof(MainPage), new PropertyMetadata(true));



        public MainPage()
        {
            Current = this;
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

            ////phone
            //if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            //{
            //    StatusBar statusBar = StatusBar.GetForCurrentView();
            //    statusBar.ForegroundColor = Colors.Black;
            //}

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
            SizeChanged += (s, e) => { 
                if (!DRAW.CheckMatrix())
                {
                    DRAW.ResizePanel(); 
                } 
            };
#if !DEBUG
               Pin.Visibility = Visibility.Collapsed;
               Exit.Visibility = Visibility.Collapsed;
#endif
            KeyDown += async (s, e) => {
                if (!DRAW.IsFocus || DRAW.State != View.DrawPanel.MyEnum.None)
                {
                    return;
                }
                if (!Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
                {
                    return;
                }
                vm.CurrentTools.OnToolState(DRAW, false);
                switch (e.OriginalKey)
                {
                    case VirtualKey.C: await vm.Copy(); break;
                    case VirtualKey.V: await vm.Pasts(); break;
                    case VirtualKey.Z: Exec.Undo(); break;
                    default: break;
                }
                await Task.Delay(10);
                vm.CurrentTools.OnToolState(DRAW, true);
            };
        }




        public void OnCreate(object sender, RoutedEventArgs e)
        {
            new LayerPaint.NewFileDialog(vm).ShowMux();//新画板
        }
        public async void OnSave(object sender, RoutedEventArgs e)
        {
            var W = Convert.ToInt32(vm.DrawRect.Width);
            var H = Convert.ToInt32(vm.DrawRect.Height);
            await vm.SaveFile(await vm.CurrentFile, vm.LayerList, W, H);
            vm.backup();
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
            var files = await openPicker.PickMultipleFilesMux();
            //var files = await ShowDialog(openPicker.PickMultipleFilesAsync);

            vm.Loading = true;
            List<LayerModel> ls = new List<LayerModel>();
            foreach (var file in files)
            {
                await vm.LoadFile(file, ls);
            }
            if (ls.Count != 0)
            {
                ls.Reverse();
                Exec.Do(new Exec() {
                    exec = delegate {
                        foreach (var item in ls)
                            vm.LayerList.Insert(0, item);
                        vm.CurrentLayer = ls[0];
                    },
                    undo = delegate {
                        foreach (var item in ls)
                            vm.LayerList.Remove(item);
                    }
                });
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
            var file = await picker.PickSaveFileMux();
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

        private void BarLayer_Tapped(object sender, RoutedEventArgs e)
        {
            DRAW.FlipPanel();
        }

        private void AppBarButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            new LayerPaint.ColorDialog(Model.VModel.vm).ShowMux();
        }
        private void AppBarButton_DragTapped(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.IsInContact)
            {
                new LayerPaint.ColorDialog(Model.VModel.vm).ShowMux();
            }
        }

        private void AppBarButton_RightTapped_1(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }
        private void AppBarButton_DragTapped_1(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.IsInContact)
            {
                FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
            }
        }

        private void AppBarButton_RightTapped_2(object sender, RightTappedRoutedEventArgs e)
        { 
            DRAW.ResizePanel();
        }
        private void AppBarButton_DragTapped_2(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.IsInContact)
            {
                DRAW.ResizePanel();
            }
        } 
    }

    static class T
    {
        static Task a = Task.FromResult(0);
        static Task<T> aa<T>(Func<Task<T>> ff)
        {
            var c = new TaskCompletionSource<T>();
            a.ContinueWith(x => {
                _ = MainPage.Current.Dispatcher.RunIdleAsync(async xx => {
                    c.SetResult(await ff());
                });
            });
            return (a = c.Task) as Task<T>; 
        }

        public static Task<IUICommand> ShowMux(this MessageDialog s)
        {
            return aa(async () => await s.ShowAsync());
        }
        public static Task<ContentDialogResult> ShowMux(this ContentDialog s)
        {
            return aa(async () => await s.ShowAsync());
        }
        public static Task<StorageFile> PickSaveFileMux(this FileSavePicker s)
        {
            return aa(async () => await s.PickSaveFileAsync());
        }
        public static Task<StorageFile> PickSingleFileMux(this FileOpenPicker s)
        {
            return aa(async () => await s.PickSingleFileAsync());
        }
        public static Task<IReadOnlyList<StorageFile>> PickMultipleFilesMux(this FileOpenPicker s)
        {
            return aa(async () => await s.PickMultipleFilesAsync());
        }
    }
    class DelayRun
    {
        int bb = 0;
        public int delay = 300;
        public void ck(Action f)
        {
            if (bb++ == 0) ((Action)async delegate {
                while (true)
                {
                    await Task.Delay(delay);
                    if (bb == 1)
                    {
                        f();
                        bb = 0;
                        return;
                    }
                    bb = 1;
                }
            })();
        }
    }
}
