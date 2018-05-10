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
        public PickerModel() :base()
        {
            Icon = "ms-appx:///Assets/AppBar/picker.png";
            Name = "picker";

        }
        bool loc = false;
        public override void OnDrawing(IModel sender, PointerPoint args)
        {
            var layer = sender.CurrentLayer;
            if (loc || layer.Bitmap==null) return;
            loc = true;
            Dispatcher.RunIdleAsync(_ => {
                var p = new Point(-layer.X + args.Position.X, -layer.Y + args.Position.Y);// getPosition(e);
                var c = layer.Bitmap.getColor((int)p.X, (int)p.Y); ;
                c.A = 255;
                MainPage.Current.MainColor = c;
                loc = false;
            }).ToString();
        }

    }
}
