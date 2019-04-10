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
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using App2.View;
using Windows.UI.Xaml.Controls;

namespace App2.Model.Tools
{
    class MoveModel : ToolsModel
    {
        public MoveModel():base()
        {
            Icon = "ms-appx:///Assets/AppBar/move.png";
            Name = "resize";
        }
        Point a;
        Point o;
        Task<WriteableBitmap> tt;
        WriteableBitmap cbmp;
        WriteableBitmap obmp;
        
        public override async void OnDrawBegin(IModel sender, PointerPoint args)
        {
            var layer = sender.CurrentLayer;
            if (Clipper.IsCliping)
            {
                layer.getRect(out Rect orec, out WriteableBitmap obb);
                var Bitmap = new WriteableBitmap((int)DrawRect.Width, (int)DrawRect.Height);
                IGrap.copyImg(obb, Bitmap, (int)orec.X, (int)orec.Y);

                layer.Child = Clipper.OnCreateArea(Bitmap);

                o = (Point)layer.Child.Tag;
                a = (Vec2)args.Position;


                obmp = layer.Bitmap;
                cbmp = await (tt = layer.Child.Render());
                if (layer.Bitmap != null)
                {
                    layer.Bitmap = layer.Bitmap.Clone();
                    IGrap.delImg(cbmp, layer.Bitmap, (int)(o.X - orec.X), (int)(o.Y - orec.Y));
                    layer.Bitmap.Invalidate(); 
                }
                //((DrawPanel)sender).X.Source = cbmp;
            }
            else
            {
                o = layer.Point;
                a = args.Position;
            }

        }
        public override void OnDrawing(IModel sender, PointerPoint args)
        {
            var layer = sender.CurrentLayer;
            if (Clipper.IsCliping)
            {
                var p = (Vec2)args.Position - a;
                var pos = (CompositeTransform)layer.Child.RenderTransform;
                pos.TranslateX = p.x - layer.X;
                pos.TranslateY = p.y - layer.Y;

            }
            else
            {
                layer.Point = (Vec2)o + args.Position - a;
            }
        }

        public override async void OnDrawCommit(IModel sender, PointerPoint args)
        {
            var layer = sender.CurrentLayer;
            if (Clipper.IsCliping)
            { 
                VModel.vm.Loading = true;
                await tt; 
                VModel.vm.Loading = false; 
                layer.Child = null;
                Clipper.Points.Clear();

                var p = (Vec2)args.Position - a + o;
                var i = sender.Layers.IndexOf(layer);
                var ob = this.obmp;
                var nb = sender.CurrentLayer.Bitmap.Clone();

                Exec.Do(new LayerPaint.Exec() {
                    exec = () => {
                        var n = new LayerModel() { Bitmap = cbmp, X = p.x, Y = p.y };
                        sender.CurrentLayer = sender.Layers[i];
                        sender.CurrentLayer.Bitmap = nb;
                        sender.Layers.Insert(i, n);
                        sender.CurrentLayer = n;
                    },
                    undo = () => {
                        sender.Layers.RemoveAt(i);
                        sender.CurrentLayer = sender.Layers[i];
                        sender.CurrentLayer.Bitmap = ob;
                    }
                });
            }
            else
            {
                var oo = o;
                var aa = layer.Point;
                Exec.Save(new Exec() {
                    exec = delegate {
                        layer.Point = aa;
                    },
                    undo = delegate {
                        layer.Point = oo;
                    }
                });
            }
        }
        public override async void OnDrawRollback(IModel sender, PointerPoint args)
        {
            var layer = sender.CurrentLayer;
            if (Clipper.IsCliping)
            {
                VModel.vm.Loading = true;
                await tt;
                VModel.vm.Loading = false;
                layer.Child = null; 
                layer.Bitmap = obmp;
            }
            else
            {
                layer.Point = o;
            }
        }

    }
}
