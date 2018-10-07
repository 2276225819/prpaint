using App2.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace App2.View
{
    public sealed partial class ToolsList : UserControl
    {
        public ClipModel Clipper { get; set; }
        public ToolsModel CurrentTools
        {
            get { return VModel.vm.CurrentTools; }
            set { VModel.vm.CurrentTools = value; }
        }


        public ToolsList()
        {
            this.InitializeComponent();
            Dispatcher.RunIdleAsync((_) => {
                Extends.Add(Clipper);

                Extends.Add(new Model.Tools.ResizeModel());
                Extends.Add(new Model.Tools.PickerModel());

                Extends.Add(new Model.Tools.BuckerModel());
                Extends.Add(new Model.Tools.TextModel());
                Extends.Add(new Model.Tools.FillModel());
                //Extends.Add(new ToolsModel() {
                //    Icon = "ms-appx:///Assets/AppBar/bucker.png",
                //    Name = "bucker",
                //});
                //Extends.Add(new ToolsModel() {
                //    Icon = "ms-appx:///Assets/AppBar/text.png",
                //    Name = "text",
                //});

                VModel.vm.RegisterPropertyChangedCallback(VModel.CurrentToolsProperty, (s, e) => {
                    SetSelected((ToolsModel)s.GetValue(e));
                });
            }).ToString();
        }

        public ObservableCollection<ToolsModel> Items { get; set; }
        public ObservableCollection<ToolsModel> Extends { get; set; } = new ObservableCollection<ToolsModel>();


        private void SetSelected(ToolsModel sender)
        {
            A.SelectedItem = null;
            B.SelectedItem = null;
            A.SelectedItem = sender;
            B.SelectedItem = sender;
        }
        private void SelectionChanged(object sender, RoutedEventArgs e)
        {
            var item = (sender as ListViewBase).SelectedItem as ToolsModel;
            if (CurrentTools == item || null == item)
            {
                return;
            } 
            CurrentTools = item;
        }

        private void UserControl_Loading(FrameworkElement sender, object args)
        {
            CurrentTools = CurrentTools;//bug
        }
    }
}
