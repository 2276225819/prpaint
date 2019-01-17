using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using LayerPaint;
using Windows.Foundation;

namespace App2.Model.Tools
{
    class PickerModel : ToolsModel
    {
        public bool PickAll
        {
            get { return (bool)GetValue(PickAllProperty); }
            set { SetValue(PickAllProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PickAll.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PickAllProperty =
            DependencyProperty.Register("PickAll", typeof(bool), typeof(PickerModel), new PropertyMetadata(true));


        public PickerModel() : base()
        {
            Icon = "ms-appx:///Assets/AppBar/picker.png";
            Name = "picker";
        }
        WriteableBitmap bmp;
        public override void OnToolState(IModel sender, bool args)
        {
            Debug.WriteLine("picker");
            if (PickAll)
            {
                bmp = new WriteableBitmap((int)DrawRect.Width, (int)DrawRect.Height);
                foreach (var item in sender.Layers.Reverse())
                {
                    if (!item.IsShow) continue;
                    IGrap.addImg(item.Bitmap, bmp, (int)item.X, (int)item.Y, item.Opacity);
                }
            }
            else
            {
                bmp = new WriteableBitmap((int)DrawRect.Width, (int)DrawRect.Height);
                var item = sender.CurrentLayer;
                IGrap.addImg(item.Bitmap, bmp, (int)item.X, (int)item.Y, item.Opacity);
            }

        } 

        bool loc = false;
        public override void OnDrawing(IModel sender, PointerPoint args)
        {
            var layer = sender.CurrentLayer;
            if (loc || bmp == null) return;
            loc = true;


            Dispatcher.RunIdleAsync(_ => {
                var p = args.Position;// new Point(-layer.X + args.Position.X, -layer.Y + args.Position.Y);// getPosition(e);
                var c = bmp.getColor((int)p.X, (int)p.Y); ;
                c.A = 255;
                MainPage.Current.MainColor = c;
                loc = false;
            }).ToString();
        }

    }
}
