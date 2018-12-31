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
        public override void OnToolChange(IModel sender, bool state)
        {
            OnLayerChange(sender);
        }

        public override void OnLayerChange(IModel sender)
        {
            tmpModel = sender;
            var t = sender.CurrentLayer.Name.Split('\t');
            Text = t[0];
            Size = t.Length > 1 ? double.Parse(t[1]) : 10;
            var ff = t.Length > 2 ? t[2].TrimEnd('\0') : "";
            if(Fonts.IndexOf(ff) != -1){
                FontName = ff;
            }
            else
            {
                FontName = Fonts[0];
            }
            Debug.Write("ChangeName");
        }

        public override void OnDrawing(IModel sender, PointerPoint args)
        {
            var e = sender.ElemArea.Child as TextBlock;
            if (e == null) return;
            e.RenderTransform = new TranslateTransform() { X = args.Position.X  , Y = args.Position.Y };
        }

        public override void OnDrawRollback(IModel sender, PointerPoint args)
        {
            base.OnDrawRollback(sender, args);
            sender.ElemArea.Child = null;
        }

        public override void OnDrawBegin(IModel sender, PointerPoint args)
        {
            if (Text == null || Text.Length < 1) return;
            base.OnDrawBegin(sender, args);
            Clipper.Points.Clear();
            sender.ElemArea.Child = Elem<TextBlock>(e => {
                e.Text = Text;
                e.FontFamily = new FontFamily(FontName);
                e.Foreground = new SolidColorBrush() { Color = Color };
                //e.HorizontalAlignment = HorizontalAlignment.Center;
                //e.HorizontalTextAlignment = TextAlignment.Left;
                //e.VerticalAlignment = VerticalAlignment.Center;
                e.FontSize = Size;
            }); 
        }

        public override async void OnDrawCommit(IModel sender, PointerPoint args)
        {
            base.OnDrawCommit(sender, args);
            var area = sender.ElemArea as Border;
            if (area.Child == null) return;

            VModel.vm.Loading = true;
            await doing(sender, area.Child as FrameworkElement);
            VModel.vm.Loading = false;
            sender.ElemArea.Child = null;

        }

        async Task doing(IModel sender,FrameworkElement child)
        {
            var rb = await child.Render();
            if (rb == null) return;

            sender.CurrentLayer.getRect(out Rect or, out WriteableBitmap ob);
            var layer = sender.CurrentLayer;
            var pos = child.RenderTransform as TranslateTransform;
            var rect = new Rect(pos.X, pos.Y, rb.PixelWidth, rb.PixelHeight);
            var nr = RectHelper.Intersect(rect, DrawRect);
            if (nr.IsEmpty) return;

            var i = sender.Layers.IndexOf(layer);
            var b = sender.CurrentLayer.Bitmap.Clone();

            var otxt = sender.CurrentLayer.Name;
            var ntxt = Text + "\t" + Size + "\t" + FontName;

            var nb = new WriteableBitmap((int)Math.Ceiling(nr.Width), (int)Math.Ceiling(nr.Height));
            // IGrap.addImg(b, nb, -(int)Math.Floor(nr.X - or.Left), -(int)Math.Floor(nr.Y - or.Top));
            IGrap.addImg(rb, nb, -(int)Math.Floor(nr.X - rect.X), -(int)Math.Floor(nr.Y - rect.Y));


            Exec.Do(new Exec() {
                exec = () => {
                    sender.Layers[i].setRect(nr, nb);
                    sender.Layers[i].Name = ntxt; ;
                },
                undo = () => {
                    sender.Layers[i].setRect(or, ob);
                    sender.Layers[i].Name = otxt;
                    sender.CurrentLayer = sender.Layers[i];
                }
            });

        }

         
        public async void OnReflush()
        {
            if (  tmpModel?.CurrentLayer?.Bitmap == null || Text==null) return;
            await Task.Delay(100);//还没更新值
            Debug.WriteLine("Text OnReflush");

            Clipper.Points.Clear();
            var area = Elem<TextBlock>(e => {
                e.Text = Text;
                e.FontFamily = new FontFamily(FontName);
                e.Foreground = new SolidColorBrush() { Color = Color };
                //e.HorizontalAlignment = HorizontalAlignment.Center;
                //e.HorizontalTextAlignment = TextAlignment.Left;
                //e.VerticalAlignment = VerticalAlignment.Center;
                e.RenderTransform = new TranslateTransform() { X = tmpModel.CurrentLayer.X, Y = tmpModel.CurrentLayer.Y };
                e.FontSize = Size;
            });
            tmpModel.ElemArea.Child = area;
            await doing(tmpModel, area);
            tmpModel.ElemArea.Child = null;
             

        }
    }
}
