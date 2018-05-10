using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace App2.View
{
    public sealed partial class ColorList : UserControl
    {
        public SolidColorBrush MainBrush { get; set; } 
        public SolidColorBrush BackBrush { get; set; }       

        public ObservableCollection<SolidColorBrush> Items { get; set; }

        public ColorList()
        {
            this.InitializeComponent();

            Dispatcher.RunIdleAsync((s) => {
                if (MainBrush == null) return;
                var c = MainBrush.Color;
                T.Text = string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
                MainBrush.RegisterPropertyChangedCallback(SolidColorBrush.ColorProperty, (obj, prop) => {
                    c = (Color)obj.GetValue(prop);
                    T.Text = string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
                });
            }).ToString();
        }



        private void GridView_DoubleTapped(object sender,  RoutedEventArgs e)
        { 
            Items.Add(new SolidColorBrush() {
                Color = MainBrush.Color
            });
        }
        private void Border_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (Items.Count > 1)
                Items.Remove((sender as FrameworkElement).DataContext as SolidColorBrush); 
        }

        private FrameworkElement Hover;
        private void Border_Holding(object sender, HoldingRoutedEventArgs e)
        {
            Hover = (FrameworkElement)sender;
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var nc = ((sender as Grid).Background as SolidColorBrush).Color;
            MainBrush.Color = nc;
            e.Handled = true;
        }
        private void Border_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Border_Holding(sender, null);
        }
        private void Insert_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var index = Items.IndexOf((Hover as Grid).Background as SolidColorBrush);
            if (index != -1)
            {
                Items.Insert(index, new SolidColorBrush() {
                    Color = MainBrush.Color
                });
            }
        }
        private void Delete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Items.Count > 1)
                Items.Remove((Hover as FrameworkElement).DataContext as SolidColorBrush);
        }
        private void Edit_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ((Hover as FrameworkElement).DataContext as SolidColorBrush).Color = MainBrush.Color;
        }



        public void SwitchColor(object sender, RoutedEventArgs e)
        {
            var c = BackBrush.Color;
            BackBrush.Color = MainBrush.Color;
            MainBrush.Color = c;
        }

        private void Button_Click(object sender, TappedRoutedEventArgs e)
        {
            if (GLIST.SelectedIndex >= 0)
            {
                (GLIST.SelectedItem as SolidColorBrush).Color = MainBrush.Color;
            }
            else
            {
                Items.Add(new SolidColorBrush() {
                    Color = MainBrush.Color
                });
            }
        }

        private void Button_Click_1(object sender, TappedRoutedEventArgs e)
        {

            if (GLIST.SelectedIndex >= 0)
            {
                Items.RemoveAt(GLIST.SelectedIndex);
            } 
        }

        private void GLIST_Tapped(object sender, RoutedEventArgs e)
        {
            GLIST.SelectedIndex = -1;
        }

        private void OpenColor(object sender, PointerRoutedEventArgs e)
        {
            var d = new LayerPaint.ColorDialog(Model.VModel.vm);
            d.ShowAsync().AsTask();
        }
    }
}
