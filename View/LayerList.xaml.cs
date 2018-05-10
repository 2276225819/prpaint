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
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using Windows.Storage;
using App2.Model;
using Windows.Storage.Pickers;
using LayerPaint;
using System.Diagnostics;
using System.Threading.Tasks;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace App2.View
{
    class LayerCC
        :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (int)((double)value * 100) + "%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public sealed partial class LayerList : UserControl 
    {      
        public ObservableCollection<LayerModel> Items { get; set; }
        public LayerModel CurrentLayer
        {
            get { return VModel.vm.CurrentLayer; }
            set { VModel.vm.CurrentLayer = value; }
        }
         


        public LayerList()
        {
            this.InitializeComponent();
            Dispatcher.RunIdleAsync(_ => { 
                Items.CollectionChanged += async (s, e) => {
                    await Task.Delay(100);
                    SetSelected(VModel.vm.CurrentLayer); 
                };
                VModel.vm.RegisterPropertyChangedCallback(VModel.CurrentLayerProperty, (s, e) => {
                    SetSelected(VModel.vm.CurrentLayer);
                });
            }).ToString();
        } 


        private void ViewIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var elem = sender as SymbolIcon;
            var model = elem.DataContext as LayerModel;
            model.IsShow = !model.IsShow;
        }
        private void EditIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var elem = sender as SymbolIcon;
            var model = elem.DataContext as LayerModel;
            model.IsEdit = !model.IsEdit;
        }
        private void SetSelected(LayerModel sender)
        {
            A.SelectedItem = sender;
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as ListViewBase).SelectedItem as LayerModel;
            if (CurrentLayer == item || null == item)
            {
                return;
            }
            CurrentLayer = item;
        }

        private void A_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentLayer = CurrentLayer;//bug
        }
    }
}
