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
using System.Runtime.InteropServices.WindowsRuntime;

namespace App2.Model.Tools
{
    class BuckerModel : ToolsModel
    {
        public bool PickAll
        {
            get { return (bool)GetValue(PickAllProperty); }
            set { SetValue(PickAllProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PickAll.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PickAllProperty =
            DependencyProperty.Register("PickAll", typeof(bool), typeof(PickerModel), new PropertyMetadata(true));

        public BuckerModel() : base()
        {
            Name = "Bucker";
            Icon = "ms-appx:///Assets/AppBar/bucker.png";
        }
        public override async void OnDrawCommit(IModel sender, PointerPoint args)
        {
            VModel.vm.Loading = true;

            var gdi = new Graphics();
            gdi.Color = Color;

            var layer = sender.CurrentLayer;
            layer.getRect(out Rect orec, out WriteableBitmap obmp);


            var nbmp = new WriteableBitmap((int)DrawRect.Width, (int)DrawRect.Height);
            var mask = new WriteableBitmap((int)DrawRect.Width, (int)DrawRect.Height);
            var nrec = new Rect(0, 0, nbmp.PixelWidth, nbmp.PixelHeight);

            IGrap.copyImg(obmp, nbmp, (int)orec.X, (int)orec.Y);
            if (PickAll)
            {
                foreach (var item in sender.Layers.Reverse())
                {
                    if (!item.IsShow) continue;
                    IGrap.addImg(item.Bitmap, mask, (int)item.X, (int)item.Y, item.Opacity);
                }
            }
            else
            {
                IGrap.copyImg(obmp, mask, (int)orec.X, (int)orec.Y);
            }



            var p = new Point(args.Position.X, args.Position.Y);

            if (Clipper.IsCliping)
            {
                var Bitmap = new WriteableBitmap((int)DrawRect.Width, (int)DrawRect.Height);
                gdi.SetBitmap(Bitmap);
                layer.setRect(nrec, nbmp);//bug bug bug
                await gdi.DrawBucker((int)p.X, (int)p.Y, mask.PixelBuffer.ToArray());
                //await Task.Delay(1000);
                layer.Child = Clipper.OnCreateArea(Bitmap);
                var bmp = await layer.Child.Render();
                var pos = (Point)layer.Child.Tag;
                IGrap.addImg(bmp, nbmp, (int)Math.Round(pos.X), (int)Math.Round(pos.Y));
                layer.Child = null;
                nbmp.Invalidate();//bug bug bug
            }
            else
            {
                gdi.SetBitmap(nbmp);
                await gdi.DrawBucker((int)p.X, (int)p.Y, mask.PixelBuffer.ToArray());
            }
            var i = sender.Layers.IndexOf(layer);
            Exec.Do(new Exec() {
                exec = () => {
                    sender.Layers[i].setRect(nrec, nbmp);
                },
                undo = () => {
                    sender.Layers[i].setRect(orec, obmp);
                }
            });
            VModel.vm.Loading = false;
        }
    }
}
