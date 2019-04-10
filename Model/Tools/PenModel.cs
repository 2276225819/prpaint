using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input;
using Windows.UI.Xaml;
using LayerPaint;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using Windows.Storage.Streams;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using System.Threading;

namespace App2.Model.Tools
{
    class PenModel : ToolsModel
    {
        public override string Icon { get; set; } = "ms-appx:///Assets/AppBar/edit.png";

        //static App2.G2018 gdi = new App2.G2018();
        static Graphics8 gdi = new Graphics8();

        Vec2 p1, p2, p;
        public float hard { get; set; } = 0.5f;
        public float opacity { get; set; } = 1; //底色透明度
        public int size { get; set; } = 10;
        public double range { get; set; } = 50;

        public float density { get; set; } = 1; //层叠深度
        public float blend { get; set; } = 0;
        public float dilution { get; set; } = 0;

        public float size_min { get; set; } = 0;
        public float density_min { get; set; } = 0;
        public float blend_min { get; set; } = 0;

        public bool size_prs { get; set; } = true;
        public bool density_prs { get; set; } = false;
        public bool blend_prs { get; set; } = false;

        public PenModel()
        {

        }
        public PenModel(string[] arr) 
        {
            try
            {
                int i = 1;
                Name = arr[i++];
                 
                size = Convert.ToInt32(arr[i++]);
                hard = Convert.ToSingle(arr[i++]);
                opacity = 1; i++;// Convert.ToSingle(arr[i++]);
                density = Convert.ToSingle(arr[i++]);

                range = Convert.ToInt32(arr[i++]);
                blend = Convert.ToSingle(arr[i++]);
                dilution = Convert.ToSingle(arr[i++]);

                size_min = Convert.ToSingle(arr[i++]);
                density_min = Convert.ToSingle(arr[i++]);
                blend_min = Convert.ToSingle(arr[i++]);

                size_prs = Convert.ToBoolean(arr[i++]);
                density_prs = Convert.ToBoolean(arr[i++]);
                blend_prs = Convert.ToBoolean(arr[i++]);
            }
            catch (Exception)
            {
                //兼容旧代码
            }
        }
 

        public override string ToData()
        {
            return base.ToData() + "|" + Name
                + "|" + size + "|" + hard + "|" + opacity + "|" + density
                + "|" + range + "|" + blend + "|" + dilution
                + "|" + size_min + "|" + density_min + "|" + blend_min
                + "|" + size_prs + "|" + density_prs + "|" + blend_prs;
        }

        static WriteableBitmap obmp; 
        static bool merge;
        static Rect orec;
        static Rect rect;
        public override void OnDrawBegin(IModel sender, PointerPoint args)
        {
            var f = args.Properties.Pressure;
            var s = size_prs ? Math.Max(size * size_min, size * f) : size;
            var d = density_prs ? Math.Max(density * density_min, density * f) : density;
            var b = blend_prs ? Math.Max(blend * blend_min, blend * (1 - f)) : blend;
            var layer = sender.CurrentLayer;
            layer.getRect(out orec, out obmp);
            layer.setRect(DrawRect, obmp, orec);


            if (Clipper.IsCliping)
            { 
                var Bitmap = new WriteableBitmap((int)DrawRect.Width, (int)DrawRect.Height);
                layer.Child = Clipper.OnCreateArea(Bitmap); 

                gdi.SetBitmap(Bitmap);
                gdi.Color = Color;
                gdi.Size = s;
            }
            else
            {
                gdi.SetBitmap(layer.Bitmap);
                gdi.Color = Color;
                gdi.Size = s;
                //gdi.Hard = hard;
                //gdi._lsize = size * args.Properties.Pressure; 
            }



            merge = false;
            p = p1 = p2 = new Point(-layer.X + args.Position.X, -layer.Y + args.Position.Y);// getPosition(e);
            rect = new Rect((p.x - s), (p.y - s), (s * 2) + 2, (s * 2) + 2);

           // View.DrawPanel.rec.setRect(rect);
            gdi.DrawCircle(p, s, hard, opacity, d, b, dilution);
            gdi.mm = p;
        }
        public override void OnDrawing(IModel sender, PointerPoint args)
        {
            var f = args.Properties.Pressure;
            var s = size_prs ? Math.Max(size * size_min, size * f) : size;
            var d = density_prs ? Math.Max(density * density_min, density * f) : density;
            var b = blend_prs ? Math.Max(blend * blend_min, blend * (1 - f)) : blend;
            var layer = sender.CurrentLayer;

            var pr = new Rect((p.x - s), (p.y - s), (s * 2) + 2, (s * 2) + 2);
            if (!merge && orec != RectHelper.Union(orec, pr))
                merge = true;

            p = new Point(-layer.X + args.Position.X, -layer.Y + args.Position.Y);// getPosition(e);
            rect = RectHelper.Intersect(RectHelper.Union(rect, pr), DrawRect);
            if (Vec2.testLen(p2, p, s * Graphics.Step))
            {
                //gdi.DrawBerzier(p2, p1, p);
                gdi.DrawBerzier(p2, p1, p, s, hard, opacity, d, b, dilution);
                p2 = p1; p1 = p;
            }


           // View.DrawPanel.rec.setRect(rect);
        }
        public override async void OnDrawCommit(IModel sender, PointerPoint args)
        {
            if (rect.IsEmpty)
            {
                OnDrawRollback(sender, args);
                return;
            }
            var layer = sender.CurrentLayer;
            if (Clipper.IsCliping)
            {
                //sender.X.Source = bmp; 
                VModel.vm.Loading = true;
                var bmp = await layer.Child.Render();
                VModel.vm.Loading = false;

                var pos = (Point)layer.Child.Tag;
                IGrap.addImg(bmp, layer.Bitmap, (int)Math.Round(pos.X), (int)Math.Round(pos.Y));

                layer.Child = null;
            }
            if (merge)
            {
                var or = orec;
                var ob = obmp;
                var nr = RectHelper.Intersect(RectHelper.Union(rect, orec), DrawRect);
                var i = sender.Layers.IndexOf(layer);
                var nb = IGrap.clipImg(layer.Bitmap, nr);
                Exec.Do(new Exec() {
                    exec = () => { 
                        sender.Layers[i].setRect(nr, nb);
                    },
                    undo = () => {
                        sender.Layers[i].setRect(or, ob);
                        sender.CurrentLayer = sender.Layers[i];
                    }
                });
            }
            else
            {
                var or = orec;
                var nr = rect;
                var nb = IGrap.clipImg(layer.Bitmap, nr);

                layer.setRect(orec, obmp);
                nr.X -= or.X;
                nr.Y -= or.Y;
                var ob = IGrap.clipImg(obmp, nr);
                var i = sender.Layers.IndexOf(layer);
                Exec.Do(new Exec() {
                    exec = () => {
                        layer = sender.Layers[i];
                        IGrap.copyImg(nb, layer.Bitmap, (int)Math.Floor(nr.X), (int)Math.Floor(nr.Y));
                        layer.Bitmap.Invalidate();
                    },
                    undo = () => {
                        layer = sender.Layers[i];
                        IGrap.copyImg(ob, layer.Bitmap, (int)Math.Floor(nr.X), (int)Math.Floor(nr.Y));
                        layer.Bitmap.Invalidate();
                        sender.CurrentLayer = sender.Layers[i];
                    }
                });
            }
        }
        public override void OnDrawRollback(IModel sender, PointerPoint args)
        {
            var layer = sender.CurrentLayer;
            layer.setRect(orec, obmp);
            if (Clipper.IsCliping)
            {
                layer.Child = null;
            }
        }


    }
}
