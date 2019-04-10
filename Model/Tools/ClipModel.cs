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
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

namespace App2.Model
{
    public class ClipModel : ToolsModel
    {
        Action<PointerPoint> Drawing;
        Action<PointerPoint> Commit;
        Action<PointerPoint> Rollback;
        Action Clear;
        Func<WriteableBitmap, FrameworkElement> Create;

        public string ClipType
        {
            get { return (string)GetValue(ClipTypeProperty); }
            set { SetValue(ClipTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClipType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClipTypeProperty =
            DependencyProperty.Register("ClipType", typeof(string), typeof(ClipModel), new PropertyMetadata("Rect"));

        public ClipModel() : base()
        {
            if (Clipper != null)
            {
                throw null;
            }
            Clipper = this;

            Icon = "ms-appx:///Assets/AppBar/selection.png";
            Name = "clipper";
        }
        public override void OnDrawBegin(IModel sender, PointerPoint _)
        {
            if (ClipType == "Rect")
            {

                return;
            }
            if (ClipType == "Free")
            {
                PointCollection Points = new PointCollection();
                List<Point> bk = Points.ToList();
                double vv = 1;
                FrameworkElement CreateTmpPolygon(IModel m, PointCollection p)
                {
                    var v = new Polygon()
                    {
                        Points = Points = p,
                        StrokeLineJoin = PenLineJoin.Bevel,
                        StrokeThickness = vv * 2,//
                        StrokeDashArray = new DoubleCollection() { 4 },
                        Stroke = Elem<LinearGradientBrush>(_ =>
                        {
                            for (double i = 0; i < 10; i++)
                            {
                                _.GradientStops.Add(new GradientStop()
                                {
                                    Color = i % 2 == 0 ? Colors.White : Colors.Black,
                                    Offset = i * 0.1
                                });
                            }
                        })
                    };
                    m.OnChangeDraw = (s) =>
                    {
                        vv = v.StrokeThickness = 1 / s;
                    };
                    return v;
                }
                sender.ElemArea.Child = CreateTmpPolygon(sender, new PointCollection());
                Rollback = args =>
                {
                    var p = new PointCollection();
                    foreach (var item in bk)
                    {
                        p.Add(item);
                    }
                    sender.ElemArea.Child = CreateTmpPolygon(sender, p);
                }
                Commit = args =>
                {
                    if (Points.Count <= 2)
                    {
                        Points.Clear();
                        sender.ElemArea.Child = null;
                    }
                    PointCount = Points.Count;
                };
                Drawing = args =>
                {
                    var x = (int)args.Position.X;
                    var y = (int)args.Position.Y;
                    if (Points.Count > 0)
                    {
                        if (!Vec2.testLen(args.Position, Points.Last(), 2))
                        {
                            return;
                        }
                    }
                    Points.Add(new Point(x, y));
                };
                Create = args =>
                {
                    var points = new PointCollection();
                    var mx = Points[0].X;
                    var my = Points[0].Y;
                    points.Add(Points[0]);
                    for (int i = 1; i < Points.Count; i++)
                    {
                        if (Points[i].X < mx) mx = Points[i].X;
                        if (Points[i].Y < my) my = Points[i].Y;
                        points.Add(Points[i]);
                    }
                    return new Polygon()
                    {
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        RenderTransform = new CompositeTransform(),
                        UseLayoutRounding = false,
                        Points = points,
                        Tag = new Point(mx, my),
                        Fill = new ImageBrush()
                        {
                            ImageSource = Bitmap,
                            Stretch = Stretch.None,
                            AlignmentX = AlignmentX.Left,
                            AlignmentY = AlignmentY.Top,
                            Transform = new TranslateTransform()
                            {
                                X = -mx,
                                Y = -my,
                            }
                        }
                    };
                };
                Clear = (layer) =>{ 
                    Points.Clear();
                    layer.Child = null;
                };
                return;
            }
        }
        public override void OnDrawing(IModel sender, PointerPoint args)
        {
            Drawing?.invoke(args);
        }
        public override void OnDrawCommit(IModel sender, PointerPoint args)
        {
            Commit?.invoke(args);
        }
        public override void OnDrawRollback(IModel sender, PointerPoint args)
        {
            Rollback?.invoke(args);
        }

        public FrameworkElement OnCreateArea(WriteableBitmap Bitmap)
        {
            return Create?.invoke(Bitmap);
        }
        public void OnClearArea()
        {
            Clear?.invoke();
        }


        public bool IsCliping => Points.Count > 2;

        public async Task<WriteableBitmap> CopyImage(LayerModel layer)
        {
            layer.getRect(out Rect orec, out WriteableBitmap obb);
            if (Clipper.IsCliping)
            {
                var Bitmap = new WriteableBitmap((int)DrawRect.Width, (int)DrawRect.Height);
                IGrap.copyImg(obb, Bitmap, (int)orec.X, (int)orec.Y);

                layer.Child = Create?.invoke(Bitmap);

                var xb = await layer.Child.Render();

                OnClearArea(); 
                return xb;
            }
            else
            {
                return obb;
            }
        }

    }

}
