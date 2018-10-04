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
    class TextModel : ToolsModel
    {
        public List<string> Fonts { get; set; }
        public string Text { get; set; }
        public string FontName { get; set; }
        public double Size { get; set; }

        public TextModel() : base()
        {
            Icon = "ms-appx:///Assets/AppBar/text.png";
            Name = "text";

            Fonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies().ToList();
            FontName = Fonts[0];
            Size = 1;
        }

        public override void OnDrawing(IModel sender, PointerPoint args)
        {
            var e = sender.ElemArea.Child as TextBlock;
            if (e == null) return;
            e.RenderTransform = new TranslateTransform() { X = args.Position.X, Y = args.Position.Y };
        }
        public override void OnDrawRollback(IModel sender, PointerPoint args)
        {
            base.OnDrawRollback(sender, args);
            sender.ElemArea.Child = null;
        }


        static WriteableBitmap obmp; 
        static Rect orec;

        public override void OnDrawBegin(IModel sender, PointerPoint args)
        {
            if (Text == null || Text.Length < 1) return;
            base.OnDrawBegin(sender, args);
            Clipper.Points.Clear();
            sender.ElemArea.Child = Elem<TextBlock>(e => {
                e.Text = Text;
                e.FontFamily = new FontFamily(FontName);
                e.Foreground = new SolidColorBrush() { Color = Color };
                e.HorizontalAlignment = HorizontalAlignment.Left;
                //e.HorizontalTextAlignment = TextAlignment.Left;
                e.VerticalAlignment = VerticalAlignment.Top;
                e.FontSize = Size;
                e.RenderTransform = new TranslateTransform() { X = args.Position.X, Y = args.Position.Y };
            });

            sender.CurrentLayer.getRect(out orec, out obmp);
        }
        public override async void OnDrawCommit(IModel sender, PointerPoint args)
        {
            base.OnDrawCommit(sender, args);
            var area = sender.ElemArea as Border;
            if (area.Child == null) return;

            VModel.vm.Loading = true;

            var or = orec;
            var ob = obmp;
            var rb = await (area.Child as FrameworkElement).Render();
            var layer = sender.CurrentLayer;
            var rect = new Rect(args.Position.X,args.Position.Y, rb.PixelWidth,rb.PixelHeight);
            var nr = RectHelper.Intersect(orec.IsEmpty ? rect : RectHelper.Union(rect, orec) , DrawRect);
            var i = sender.Layers.IndexOf(layer);
            var b = sender.CurrentLayer.Bitmap.Clone();


            var nb = new WriteableBitmap((int)Math.Ceiling(nr.Width), (int)Math.Ceiling(nr.Height));
            IGrap.addImg(b, nb, -(int)Math.Floor(nr.X - or.Left), -(int)Math.Floor(nr.Y - or.Top));
            IGrap.addImg(rb, nb, -(int)Math.Floor(nr.X - args.Position.X), -(int)Math.Floor(nr.Y - args.Position.Y));


            Exec.Do(new Exec() {
                exec = () => {
                    sender.Layers[i].setRect(nr, nb);
                },
                undo = () => {
                    sender.Layers[i].setRect(or, ob);
                    sender.CurrentLayer = sender.Layers[i];
                }
            }); 
            VModel.vm.Loading = false  ;
            sender.ElemArea.Child = null;

        }
    }
}
