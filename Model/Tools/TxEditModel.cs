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
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace App2.Model.Tools
{
    class TxEditModel : ToolsModel
    {
        /* 这双向绑定真的能用??? */
        public string Text { get; set; }

        public List<string> Fonts { get; set; }
        public string FontName { get; set; }
        public double Size { get; set; }
        IModel tmpModel;

        public TxEditModel() : base()
        {
            Icon = "ms-appx:///Assets/AppBar/text.png";
            Name = "text";

            Fonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies().ToList();
            FontName = Fonts[0];
            Size = 1;
        }
        public override void OnToolState(IModel sender, bool state)
        {
            if (tmpModel == null) OnLayerChange(sender);
        }

        public override void OnLayerChange(IModel sender)
        {
            tmpModel = sender;
            var t = sender.CurrentLayer.Name.Split('\t');
            Text = t[0];
            Size = t.Length > 1 ? double.Parse(t[1]) : 10;
            var ff = t.Length > 2 ? t[2].TrimEnd('\0') : "";
            if(Fonts.IndexOf(ff) != -1)
            {
                FontName = ff;
            }
            else
            {
                FontName = Fonts[0];
            }
            Debug.Write("ChangeName"); 
            OnDrawRollback(sender, null);
            MainPage.flushtoolattr();
        }

        Point p;
        public override void OnDrawing(IModel sender, PointerPoint args)
        {
            var o = sender.CurrentLayer;
            var e = GetArea(sender);
            if (e != null) e.RenderTransform = new TranslateTransform() { X = o.X + args.Position.X - p.X, Y = o.Y + args.Position.Y - p.Y };
        }

        public override void OnDrawRollback(IModel sender, PointerPoint args)
        {
            if (sender.ElemArea.Child != null)
            {
                sender.CurrentLayer.setRect(orect, obmp);
                sender.CurrentLayer.Name = otxt;
                sender.ElemArea.Child = null;
            }
        }

        public override void OnDrawBegin(IModel sender, PointerPoint args)
        {
            p = args.Position;
        }

        public override async void OnDrawCommit(IModel sender, PointerPoint args)
        {
            VModel.vm.Loading = true;
            await Render(sender, GetArea(sender));
            VModel.vm.Loading = false;
        }

        async Task Render(IModel sender, FrameworkElement child)
        {
            if (child == null) return;
            var rb = await child.Render();
            if (rb == null) return;

            var layer = sender.CurrentLayer;
            var pos = child.RenderTransform as TranslateTransform;
            var rect = new Rect(pos.X, pos.Y, rb.PixelWidth, rb.PixelHeight);
            var or = orect;
            var nr = RectHelper.Intersect(rect, DrawRect);
            if (nr.IsEmpty) return;

            var i = sender.Layers.IndexOf(layer);
            var b = sender.CurrentLayer.Bitmap.Clone();

            var nt = (child as TextBlock).Text + "\t" + Size + "\t" + FontName;
            var ot = otxt;

            var nb = new WriteableBitmap((int)Math.Ceiling(nr.Width), (int)Math.Ceiling(nr.Height));
            var ob = obmp;
            // IGrap.addImg(b, nb, -(int)Math.Floor(nr.X - or.Left), -(int)Math.Floor(nr.Y - or.Top));
            IGrap.addImg(rb, nb, -(int)Math.Floor(nr.X - rect.X), -(int)Math.Floor(nr.Y - rect.Y));


            sender.ElemArea.Child = null;
            Exec.Do(new Exec() {
                exec = () => {
                    sender.Layers[i].setRect(nr, nb);
                    sender.Layers[i].Name = nt;
                    sender.CurrentLayer = sender.Layers[i];
                },
                undo = () => {
                    sender.Layers[i].setRect(or, ob);
                    sender.Layers[i].Name = ot;
                    sender.CurrentLayer = sender.Layers[i];
                }
            });

        }
        Rect orect;
        WriteableBitmap obmp;
        string otxt;
        TextBlock GetArea(IModel sender)
        {
            if (Text == null || Text.Length < 1)
            {
                return null;
            }
            if (sender.ElemArea.Child == null)
            {
                otxt = sender.CurrentLayer.Name;
                sender.CurrentLayer.getRect(out orect, out obmp);
                sender.CurrentLayer.Bitmap = null;
                Clipper.Points.Clear();
                var area = Elem<TextBlock>(e => {
                    e.Text = Text;
                    e.FontFamily = new FontFamily(FontName);
                    e.FontSize = Size;
                    e.Foreground = new SolidColorBrush() { Color = Color };
                    //e.HorizontalAlignment = HorizontalAlignment.Center;
                    //e.HorizontalTextAlignment = TextAlignment.Left;
                    //e.VerticalAlignment = VerticalAlignment.Center;
                    e.RenderTransform = new TranslateTransform() { X = sender.CurrentLayer.X, Y = sender.CurrentLayer.Y };
                });
                sender.ElemArea.Child = area;
            }
            return sender.ElemArea.Child as TextBlock;
        }


        bool loc = false;
        public async void OnReflush(bool render)
        {
            if (tmpModel?.CurrentLayer == null)
            {
                return;
            }
            if (loc)
            {
                return;
            }
            loc = true;
            await Task.Delay(50);//还没更新值 

            var area = GetArea(tmpModel);
            if (render)
            {
                await Render(tmpModel, area);
            }
            else
            {
                if (area != null)
                {
                    area.Text = Text;
                    area.FontFamily = new FontFamily(FontName);
                    area.FontSize = Size;
                    area.Foreground = new SolidColorBrush() { Color = Color };
                }
            }
            loc = false;
        }
    }
}
