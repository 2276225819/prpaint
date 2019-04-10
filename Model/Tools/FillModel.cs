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
                case Windows.UI.Xaml.Shapes.Line line:
                    line.RenderTransform = new TranslateTransform() {
                        X = r.Left - this.Size / 2.0,
                        Y = r.Top - this.Size / 2.0
                    };
                    line.X1 = this.Size / 2.0 - r.Left + this.op.X;
                    line.Y1 = this.Size / 2.0 - r.Top + this.op.Y;
                    line.X2 = this.Size / 2.0 - r.Left + args.Position.X;
                    line.Y2 = this.Size / 2.0 - r.Top + args.Position.Y;
                    break;
                case Windows.UI.Xaml.Shapes.Polygon pol:
                    op.X = r.Left;
                    op.Y = r.Top; 
                    pol.RenderTransform = new TranslateTransform() {
                        X = op.X - this.Size / 2.0,
                        Y = op.Y - this.Size / 2.0
                    };
                    (pol.Tag as PointCollection).Add(args.Position);
                    var np = new PointCollection();
                    foreach (var item in pol.Tag as PointCollection)
                    { 
                        np.Add(new Point(item.X - op.X + this.Size / 2.0, item.Y - op.Y + this.Size / 2.0));
                    }
                    pol.Points = np;
                    break;
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
                case "Line":
                    sender.ElemArea.Child = Elem<Windows.UI.Xaml.Shapes.Line>(e => {
                        e.Stroke = new SolidColorBrush(Color);
                        e.StrokeThickness = Size;
                        e.StrokeStartLineCap = e.StrokeEndLineCap = PenLineCap.Round;
                    });
                    break;
                case "Rectangle":
                    sender.ElemArea.Child = Elem<Windows.UI.Xaml.Shapes.Rectangle>(e => {
                        e.Stroke = new SolidColorBrush(Color);
                        e.StrokeThickness = Size;
                        if (Fill) e.Fill = new SolidColorBrush(MainPage.Current.BackColor);
                    });
                    break;
                case "Custom":
                    sender.ElemArea.Child = Elem<Windows.UI.Xaml.Shapes.Polygon>(e => {
                        e.StrokeLineJoin = PenLineJoin.Round;
                        e.Stroke = new SolidColorBrush(Color);
                        e.StrokeThickness = Size;
                        if (Fill) e.Fill = new SolidColorBrush(MainPage.Current.BackColor);
                        e.Tag = new PointCollection();

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
            OnDrawing(sender, args);
        }
        public override async void OnDrawCommit(IModel sender, PointerPoint args)
        {
            base.OnDrawCommit(sender, args);
            var area = sender.ElemArea as Border;
            if (area.Child == null) return;

            VModel.vm.Loading = true;
            sender.CurrentLayer.getRect(out Rect or, out WriteableBitmap ob);
            var rb = await (area.Child as FrameworkElement).Render();
            VModel.vm.Loading = false;
            if (rb != null)
            {
                var layer = sender.CurrentLayer;
                var pos = (area.Child as FrameworkElement).RenderTransform as TranslateTransform;
                var rect = new Rect(pos.X, pos.Y, rb.PixelWidth, rb.PixelHeight);
                var nr = RectHelper.Intersect(or.IsEmpty ? rect : RectHelper.Union(rect, or), DrawRect);
                if (nr.IsEmpty) return;
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
            sender.ElemArea.Child = null;

        }
    }
}
