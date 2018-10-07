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
    class FillModel : ToolsModel
    {
        public string Type { get; set; } = "Rectangle";
        public bool Fill { get; set; } = false;
        public double Size { get; set; } = 1;

        public FillModel() : base()
        {
            Icon = "ms-appx:///Assets/AppBar/resize.png";
            Name = "rect";
        }

        public override void OnDrawRollback(IModel sender, PointerPoint args)
        {
            sender.ElemArea.Child = null;
        }

        Point op;
        public override void OnDrawing(IModel sender, PointerPoint args)
        {
            Rect r = new Rect(op, args.Position);
            switch (sender.ElemArea.Child)
            {
                //case Windows.UI.Xaml.Shapes.Line line:
                //    line.RenderTransform = new TranslateTransform() { X = 110, Y = 110 };
                //    line.X1 = op.X;
                //    line.Y1 = op.Y;
                //    line.X2 = args.Position.X;
                //    line.Y2 = args.Position.Y;
                //    break;
                case Windows.UI.Xaml.Shapes.Rectangle rect:
                    rect.RenderTransform = new TranslateTransform() { X = r.Left, Y = r.Top };
                    rect.Width = r.Right - r.Left;
                    rect.Height = r.Bottom - r.Top;
                    break;
                case Windows.UI.Xaml.Shapes.Ellipse elli:
                    if (Type == "Circle")
                    {
                        var len = ((Vec2)op - args.Position).Length;
                        elli.RenderTransform = new TranslateTransform() { X = op.X-len, Y = op.Y-len };
                        elli.Width = elli.Height = len *2; 

                    }
                    else
                    {
                        elli.RenderTransform = new TranslateTransform() { X = r.Left, Y = r.Top };
                        elli.Width = r.Right - r.Left;
                        elli.Height = r.Bottom - r.Top;
                    }
                    break;
            }

        }
        public override void OnDrawBegin(IModel sender, PointerPoint args)
        {
            op.X = args.Position.X;
            op.Y = args.Position.Y;
            switch (Type)
            {
                //case "Line":
                //    sender.ElemArea.Child = Elem<Windows.UI.Xaml.Shapes.Line>(e => {
                //        e.Stroke = new SolidColorBrush(Color);
                //        e.StrokeThickness = Size; 
                //    });
                //    break;
                case "Rectangle":
                    sender.ElemArea.Child = Elem<Windows.UI.Xaml.Shapes.Rectangle>(e => {
                        e.Stroke = new SolidColorBrush(Color);
                        e.StrokeThickness = Size;
                        if (Fill) e.Fill = new SolidColorBrush(MainPage.Current.BackColor);
                    });
                    break;
                case "Ellipse":
                case "Circle":
                    sender.ElemArea.Child = Elem<Windows.UI.Xaml.Shapes.Ellipse>(e => {
                        e.Stroke = new SolidColorBrush(Color);
                        e.StrokeThickness = Size;
                        if (Fill) e.Fill = new SolidColorBrush(MainPage.Current.BackColor);
                    });
                    break;

            }
        }
        public override async void OnDrawCommit(IModel sender, PointerPoint args)
        {
            base.OnDrawCommit(sender, args);
            var area = sender.ElemArea as Border;
            if (area.Child == null) return;

            VModel.vm.Loading = true;

            sender.CurrentLayer.getRect(out Rect or, out WriteableBitmap ob);
            var rb = await (area.Child as FrameworkElement).Render();
            if (rb != null)
            {
                var layer = sender.CurrentLayer;
                var pos = (area.Child as FrameworkElement).RenderTransform as TranslateTransform;
                var rect = new Rect(pos.X, pos.Y, rb.PixelWidth, rb.PixelHeight);
                var nr = RectHelper.Intersect(or.IsEmpty ? rect : RectHelper.Union(rect, or), DrawRect);
                var i = sender.Layers.IndexOf(layer);
                var b = sender.CurrentLayer.Bitmap.Clone();


                var nb = new WriteableBitmap((int)Math.Ceiling(nr.Width), (int)Math.Ceiling(nr.Height));
                IGrap.addImg(b, nb, -(int)Math.Floor(nr.X - or.Left), -(int)Math.Floor(nr.Y - or.Top));
                IGrap.addImg(rb, nb, -(int)Math.Floor(nr.X - rect.X), -(int)Math.Floor(nr.Y - rect.Y));


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
            VModel.vm.Loading = false;
            sender.ElemArea.Child = null;

        }
    }
}
