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

        public PointCollection Points
        {
            get { return (PointCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Points.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(PointCollection), typeof(ClipModel), new PropertyMetadata(new PointCollection()));

        public int PointCount
        {
            get { return (int)GetValue(PointCountProperty); }
            set { SetValue(PointCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PointCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointCountProperty =
            DependencyProperty.Register("PointCount", typeof(int), typeof(ClipModel), new PropertyMetadata(0));



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
        List<Point> bk;
        public override void OnDrawBegin(IModel sender, PointerPoint args)
        {
            bk = Points.ToList();
            sender.ElemArea.Child = CreateView(sender,new PointCollection());
        }
        public override void OnDrawing(IModel sender, PointerPoint args)
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
        }
        public override void OnDrawCommit(IModel sender, PointerPoint args)
        {
            if (Points.Count <= 2)
            {
                Points.Clear();
            }
            PointCount = Points.Count;
        }
        public override void OnDrawRollback(IModel sender, PointerPoint args)
        {
            var p = new PointCollection();
            foreach (var item in bk)
            {
                p.Add(item);
            }
            sender.ElemArea.Child = CreateView(sender,p);
        }

        /// bool loc = false;
        /// public override void OnDrawing(LayerModel sender, PointerPoint args)
        /// {
        ///     if (loc || sender.Bitmap==null) return;
        ///     loc = true;
        ///     Dispatcher.RunIdleAsync(_ => { 
        ///         var p = new Point(-sender.X + args.Position.X, -sender.Y + args.Position.Y);// getPosition(e);
        ///         var c = sender.Bitmap.getColor((int)p.X, (int)p.Y); ;
        ///         c.A = 255;
        ///         MainPage.Current.MainColor = c;
        ///         loc = false;
        ///     }).ToString();
        /// }
        /// 

        public bool IsCliping => Points.Count > 2;
        public Polygon createPolygon(WriteableBitmap Bitmap)
        {
            var points = new PointCollection();
            var mx = Clipper.Points[0].X;
            var my = Clipper.Points[0].Y;
            points.Add(Clipper.Points[0]);
            for (int i = 1; i < Clipper.Points.Count; i++)
            {
                if (Clipper.Points[i].X < mx) mx = Clipper.Points[i].X;
                if (Clipper.Points[i].Y < my) my = Clipper.Points[i].Y;
                points.Add(Clipper.Points[i]);
            }
            return new Polygon() {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                RenderTransform = new CompositeTransform(),
                UseLayoutRounding = false,
                Points = points,
                Tag = new Point(mx, my),
                Fill = new ImageBrush() {
                    ImageSource = Bitmap,
                    Stretch = Stretch.None,
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Top,
                    Transform = new TranslateTransform() {
                        X = -mx,
                        Y = -my,
                    }
                }
            };
        }
        double vv = 1;
        public Polygon CreateView(IModel m,PointCollection p)
        {
            var v = new Polygon() {
                Points = Points = p,
                StrokeLineJoin = PenLineJoin.Bevel,
                StrokeThickness = vv * 2 ,//
                StrokeDashArray = new DoubleCollection() { 4 },
                Stroke = Elem<LinearGradientBrush>(_ => {
                    for (double i = 0; i < 10; i ++)
                    {
                        _.GradientStops.Add(new GradientStop() {
                            Color = i % 2 == 0 ? Colors.White : Colors.Black,
                            Offset = i*0.1
                        });
                    }
                })
            };
            m.OnChangeDraw = (s) => {
                vv = v.StrokeThickness = 1 / s;
            };
            return v;
        }
        public void movePolygon(int x, int y)
        {
            var points = new PointCollection();
            for (int i = 0; i < Clipper.Points.Count; i++)
            {
                points.Add(new Point() {
                    X = Clipper.Points[i].X + x,
                    Y = Clipper.Points[i].Y + y,
                });
            }

        }


        public async Task<WriteableBitmap> CopyImage(LayerModel layer)
        {
            layer.getRect(out Rect orec, out WriteableBitmap obb);
            if (Clipper.IsCliping)
            {
                var Bitmap = new WriteableBitmap((int)DrawRect.Width, (int)DrawRect.Height);
                //var c = Windows.UI.Color.FromArgb(0,255, 255, 255);
                //IGrap.fillColor(Bitmap, (x, y) => {
                //    return c;
                //});
                IGrap.copyImg(obb, Bitmap, (int)orec.X, (int)orec.Y);
                layer.Child = Clipper.createPolygon(Bitmap);

                var xb = await layer.Child.Render();

                Clipper.Points.Clear();
                layer.Child = null;
                return xb;
            }
            else
            {
                return obb;
            }
        }
    }
}
