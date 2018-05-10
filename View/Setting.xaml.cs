using App2.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Input;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace App1.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Setting : Page
    {
        public Setting()
        {
            this.InitializeComponent();
            if (VModel.vm.DrawType == PointerDeviceType.Pen) t0.IsOn = true;
            if (VModel.vm.BarPosition) t1.IsOn = true;
            if (VModel.vm.PanelReverse) t2.IsOn = true;
            if (VModel.vm.DrawRotation) t3.IsOn = true;
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            VModel.vm.DrawType = t0.IsOn ? PointerDeviceType.Pen : PointerDeviceType.Touch;
        }

        private void ToggleSwitch_Toggled_1(object sender, RoutedEventArgs e)
        {
            VModel.vm.BarPosition = t1.IsOn;
        }

        private void ToggleSwitch_Toggled_2(object sender, RoutedEventArgs e)
        {
            VModel.vm.PanelReverse = t2.IsOn;
        }
        private void ToggleSwitch_Toggled_3(object sender, RoutedEventArgs e)
        {
            VModel.vm.DrawRotation = t3.IsOn;
        }
    }
}
