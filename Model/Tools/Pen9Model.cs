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
using Windows.UI;
using Windows.Data.Json;

namespace App2.Model.Tools
{
    class Pen9Model : ToolsModel , IGrap9Attr
    {
        public override string Icon { get; set; } = "ms-appx:///Assets/AppBar/edit.png";


        public static Grap9 gdi = new App1.Vendor.Grap9Win2d();
        
        public JsonObject data;

        
        public double range { get { return data.GetNamedNumber(nameof(range),50); } set { data[nameof(range)] = JsonValue.CreateNumberValue(value); } }
        public float size_min { get { return (float)data.GetNamedNumber(nameof(size_min), 0); } set { data[nameof(size_min)] = JsonValue.CreateNumberValue(value); } }
        public float density_min { get { return (float)data.GetNamedNumber(nameof(density_min), 0); } set { data[nameof(density_min)] = JsonValue.CreateNumberValue(value); } }
        public bool size_prs { get { return data.GetNamedBoolean(nameof(size_prs), true); } set { data[nameof(size_prs)] = JsonValue.CreateBooleanValue(value); } }
        public bool size_fade { get { return data.GetNamedBoolean(nameof(size_fade), true); } set { data[nameof(size_fade)] = JsonValue.CreateBooleanValue(value); } }
        public bool density_prs { get { return data.GetNamedBoolean(nameof(density_prs), true); } set { data[nameof(density_prs)] = JsonValue.CreateBooleanValue(value); } }
        //public float blend_min { get { return (float)data.GetNamedNumber(nameof(blend_min), 0); } set { data[nameof(blend_min)] = JsonValue.CreateNumberValue(value); } }
        //public bool blend_prs { get { return data.GetNamedBoolean(nameof(blend_prs), false); } set { data[nameof(blend_prs)] = JsonValue.CreateBooleanValue(value); } }
        public float size { get { return (float)data.GetNamedNumber(nameof(size),2); } set { data[nameof(size)] = JsonValue.CreateNumberValue(value); } }

        public float density { get { return (float)data.GetNamedNumber(nameof(density),1); } set { data[nameof(density)] = JsonValue.CreateNumberValue(value); } } //颜色浓度
        public float blend { get { return (float)data.GetNamedNumber(nameof(blend), 0); ; } set { data[nameof(blend)] = JsonValue.CreateNumberValue(value); } } //底色浓度
        public float dilution { get { return (float)data.GetNamedNumber(nameof(dilution), 0); } set { data[nameof(dilution)] = JsonValue.CreateNumberValue(value); } }//底色透明度
        public float persistence { get { return (float)data.GetNamedNumber(nameof(persistence),0.5); } set { data[nameof(persistence)] = JsonValue.CreateNumberValue(value); } }//混色率

        public float randpos { get { return (float)data.GetNamedNumber(nameof(randpos), 0); } set { data[nameof(randpos)] = JsonValue.CreateNumberValue(value); } }
        public float randsize { get { return (float)data.GetNamedNumber(nameof(randsize), 0); } set { data[nameof(randsize)] = JsonValue.CreateNumberValue(value); } }
        public float space { get { return (float)data.GetNamedNumber(nameof(space),0.3f); } set { data[nameof(space)] = JsonValue.CreateNumberValue(value); } } 

        public float hard { get { return (float)data.GetNamedNumber(nameof(hard),0f); } set { data[nameof(hard)] = JsonValue.CreateNumberValue(value); } }
        public int randrota { get { return (int)data.GetNamedNumber(nameof(randrota), 0); } set { data[nameof(randrota)] = JsonValue.CreateNumberValue(value); } }
        public float randsharp { get { return (float)data.GetNamedNumber(nameof(randsharp), 0); } set { data[nameof(randsharp)] = JsonValue.CreateNumberValue(value); } }

        public string bgurl { get { return (string)data.GetNamedString(nameof(bgurl), ""); } set { data[nameof(bgurl)] = JsonValue.CreateStringValue(value); } }
        public string fgurl { get { return (string)data.GetNamedString(nameof(fgurl), ""); } set { data[nameof(fgurl)] = JsonValue.CreateStringValue(value); } }

        public int step
        {
            get { return (int)(VModel.Data["Pen9Step"]??3); }
            set { VModel.Data["Pen9Step"] = value; }
        }
        public Windows.UI.Color color { get => MainPage.Current.MainColor; set { } }

        public Pen9Model() { data = new JsonObject(); }
        public Pen9Model(string[] arr) 
        {
            data = JsonObject.Parse(arr[1]);
            Name = data.GetNamedString(nameof(Name),"pencil");
        }


        public override string ToData()
        {
            data[nameof(Name)] = JsonValue.CreateStringValue(Name);
            return base.ToData() + "|" + data.ToString();
        }
        public override void OnToolState(IModel sender, bool state)
        {
            cache = null;
        }

        static WriteableBitmap cache;
        static WriteableBitmap obmp; 
        static bool merge;
        static Rect orec;
        static Rect rect;
        public override void OnDrawBegin(IModel sender, PointerPoint args)
        {
            var f = args.Properties.Pressure * 2;
            var s = size_prs ? Math.Max(size * size_min, size * f) : size;
            //var d = density_prs ? Math.Max(density * density_min, density * f) : density;
            //var b = blend_prs ? Math.Max(blend * blend_min, blend * (1 - f)) : blend;
            var layer = sender.CurrentLayer;
            layer.getRect(out orec, out obmp);
            if (cache == null)
            {
                cache = new WriteableBitmap((int)DrawRect.Width, (int)DrawRect.Height);
                IGrap.copyImg(obmp, cache, (int)orec.X, (int)orec.Y);
            }
            layer.setRect(DrawRect);
            layer.Bitmap = cache;
            

            if (Clipper.IsCliping)
            { 
                var Bitmap = new WriteableBitmap((int)DrawRect.Width, (int)DrawRect.Height);
                layer.Child = Clipper.createPolygon(Bitmap);

                gdi.Init(Bitmap, this);
            }
            else
            {
                gdi.Init(cache, this); 
            }
             
            merge = false;
            var p = new Point(-layer.X + args.Position.X, -layer.Y + args.Position.Y);// getPosition(e);
            rect = new Rect((p.X - s), (p.Y - s), (s * 2) + 2, (s * 2) + 2);
             
            gdi.MoveTo((float)p.X, (float)p.Y, f);
        }

        bool b = false;
        Task t = Task.FromResult(0); 
        public override void OnDrawing(IModel sender, PointerPoint args)
        {
            var f = args.Properties.Pressure * 2;
            var s = size_prs ? Math.Max(size * size_min, size * f) : size;
            s += randpos / 2 + randsize / 2;
            //var d = density_prs ? Math.Max(density * density_min, density * f) : density;
            //var b = blend_prs ? Math.Max(blend * blend_min, blend * (1 - f)) : blend;
            var layer = sender.CurrentLayer;

            var p = new Point(-layer.X + args.Position.X, -layer.Y + args.Position.Y);// getPosition(e);
            var pr = new Rect((p.X - s), (p.Y - s), (s * 2) + 2, (s * 2) + 2);
            if (!merge && orec != RectHelper.Union(orec, pr))
                merge = true;

            rect = RectHelper.Intersect(RectHelper.Union(rect, pr), DrawRect);


            //gdi.LineTo((float)p.X, (float)p.Y, f);
            //gdi.DrawBerzier();
            //gdi.Invalidate();
            //return;
             
            t = t.ContinueWith(x => {
                gdi.LineTo((float)p.X, (float)p.Y, f);
                lock (gdi) gdi.DrawBerzier();
            });

            if (b == false && (b = true))
            {
                _ = Dispatcher.RunIdleAsync(x => {
                    lock (gdi) gdi.Invalidate();
                    b = false;
                });
            }
        }
        public override async void OnDrawCommit(IModel sender, PointerPoint args)
        {
            t.Wait();
            gdi.DrawEnd(true);
            gdi.Invalidate(); 

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
                cache = null;
            }
            if (merge || layer.Bitmap==null)
            {
                var or = orec;
                var ob = obmp;
                var nr = RectHelper.Intersect(RectHelper.Union(rect, orec), DrawRect);
                if (nr.IsEmpty)
                {
                    OnDrawRollback(sender, args);
                    return;
                }
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
                if (nr.IsEmpty || or.IsEmpty)
                {
                    OnDrawRollback(sender, args);
                    return;
                }
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
            cache = null;
        }



    }
}
