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
using Windows.UI;

namespace App2.Model.Tools
{
    class ResizeModel : ToolsModel
    {
        public ResizeModel() : base()
        {
            Icon = "ms-appx:///Assets/AppBar/move.png";
            Name = "resize";
        }
        //Point a;
        //Point o;
        //Task<WriteableBitmap> tt = null;
        //WriteableBitmap cbmp; 



        enum T { Move, Resize, Rotate, ResizeH, ResizeV,Flip, NULL };
        T type;

        Point lp;
        Point cp;
        Rect orec;
        WriteableBitmap obb;
        CompositeTransform com;
        bool ch = false;
        public override void OnDrawBegin(IModel sender, PointerPoint args)
        {
            Debug.WriteLine("BEGIN");
            com = sender.CurrentLayer.RenderTransform as CompositeTransform;
            lp = args.Position;
            cp = new Point(com.TranslateX + sender.CurrentLayer.W / 2, com.TranslateY + sender.CurrentLayer.H / 2);
        }
        public override void OnDrawCommit(IModel sender, PointerPoint args)
        {
            type = T.NULL;
        }
        public override void OnDrawRollback(IModel sender, PointerPoint args)
        {
            type = T.NULL;
        }

        public override void OnDrawing(IModel sender, PointerPoint args)
        {
            switch (type)
            {
                case T.Move:
                    var v = ((Vec2)args.Position - lp); 
                    com.TranslateX = (com.TranslateX + v.X);
                    com.TranslateY = (com.TranslateY + v.Y);
                    ch = true;
                    break;
                case T.Rotate:
                    var ag = ((Vec2)cp - lp).getAngle();
                    var _ag = ((Vec2)cp - args.Position).getAngle();
                    com.Rotation += (_ag - ag);
                    ch = true;
                    break;
                case T.Resize:
                    var ln = Vec2.getLen(cp, lp);
                    var _ln = Vec2.getLen(cp, args.Position);
                    com.ScaleX *= (_ln / ln);
                    com.ScaleY *= (_ln / ln);
                    ch = true;
                    break;
                case T.ResizeH:
                    var lnh = Vec2.getLen(cp, lp);
                    var _lnh = Vec2.getLen(cp, args.Position); 
                    com.ScaleX *= (_lnh / lnh);
                    ch = true;
                    break;
                case T.ResizeV:
                    var lnv = Vec2.getLen(cp, lp);
                    var _lnv = Vec2.getLen(cp, args.Position);
                    com.ScaleY *= (_lnv / lnv); 
                    ch = true;
                    break;
                case T.Flip:
                    var w = 5;
                    var np = args.Position;
                    var xv = np.X < lp.X - w ? +1 : (lp.X + w < np.X ? -1 : 0);
                    var yv = np.Y < lp.Y - w ? +1 : (lp.Y + w < np.Y ? -1 : 0);
                    if(xv!=0)
                    com.ScaleX = Math.Abs(com.ScaleX) * xv;
                    if(yv!=0)
                    com.ScaleY = Math.Abs(com.ScaleY) * yv;
                    break;
                default:
                    break;
            }
            lp = args.Position;
        }


        public override async void OnToolState(IModel sender, bool state)
        {
            var layer = sender.CurrentLayer;
            if ( !layer.IsEdit || !layer.IsShow)
            {
                return;
            }
            if (layer.Bitmap == null)
            {
                return;
            }
            Debug.WriteLine("state:" + state);
            if (state)
            {
                layer.getRect(out orec, out obb);
                if (Clipper.IsCliping)
                {
                    VModel.vm.Loading = true;
                    //拷贝选区
                    var p = (Point)layer.Child.Tag;
                    var xb = await Clipper.CopyImage(layer);
                    if (xb != null)
                    {
                        var i = sender.Layers.IndexOf(layer);
                        var nb = obb;
                        var ob = obb;
                        if (layer.Bitmap != null)
                        {
                            nb = layer.Bitmap.Clone();
                            IGrap.delImg(xb, nb, (int)(p.X - orec.X), (int)(p.Y - orec.Y));
                            nb.Invalidate();
                        }
                        obb = xb;
                        orec = new Rect(p.X, p.Y, xb.PixelWidth, xb.PixelHeight);
                        Exec.Do(new Exec() {
                            exec = () => {
                                sender.Layers[i].Bitmap = nb;
                                sender.Layers.Insert(i, new LayerModel() {
                                    Bitmap = xb,
                                    X = p.X,
                                    Y = p.Y
                                });
                                sender.CurrentLayer = sender.Layers[i];
                            },
                            undo = () => {
                                sender.Layers.RemoveAt(i);
                                sender.CurrentLayer = sender.Layers[i];
                                sender.CurrentLayer.Bitmap = ob;
                            }
                        });
                    }

                    VModel.vm.Loading = false;
                }
                sender.ElemArea.Child = CreateRect(sender);
                ch = false;
            }
            else
            {
                sender.ElemArea.Child = null;

                if (!ch) return;
                var vc = new CompositeTransform() {
                    Rotation = com.Rotation,
                    ScaleX = com.ScaleX,
                    ScaleY = com.ScaleY,
                    CenterX = com.TranslateX + layer.W / 2,
                    CenterY = com.TranslateY + layer.H / 2
                };

                var or = orec;
                var ob = obb;
                layer.getRect(out Rect nr, out WriteableBitmap nb);
                if (com.ScaleX != 1 || com.ScaleY != 1 || com.Rotation != 0)
                {
                    var elem = (FrameworkElement)((DrawPanel)sender).ITEMS.ContainerFromItem(layer);

                    VModel.vm.Loading = true;
                    var b = await (elem).Render();
                    VModel.vm.Loading = false;

                    var vr = vc.TransformBounds(nr);
                    nr = RectHelper.Intersect(DrawRect, vr);
                    if (nr.IsEmpty)
                    {
                        nb = null;
                        layer.setRect(nr, nb);
                    }
                    else
                    {
                        nb = new WriteableBitmap((int)nr.Width, (int)nr.Height);
                        IGrap.addImg(b, nb, (int)(vr.X - nr.X), (int)(vr.Y - nr.Y));
                        layer.setRect(nr, nb);
                    }
                    com.Rotation = 0;
                    com.ScaleX = 1;
                    com.ScaleY = 1;
                }
                var i = sender.Layers.IndexOf(sender.CurrentLayer);
                Exec.Save(new Exec() {
                    exec = () => {
                        sender.Layers[i].setRect(nr, nb);
                    },
                    undo = () => {
                        sender.Layers[i].setRect(or, ob);
                    }
                });
            }
        }


        public FrameworkElement CreateRect(IModel m)
        {
            var layer = m.CurrentLayer;
            return Elem<Grid>(grid => {
                grid.Name = "中心移动";
                grid.RenderTransform = layer.RenderTransform;
                grid.RenderTransformOrigin = new Point(0.5, 0.5);
                grid.Width = layer.W;
                grid.Height = layer.H;
                grid.BorderThickness = new Thickness(1);
                grid.BorderBrush = new SolidColorBrush() {
                    Color = Colors.Gray
                };
                grid.Background = new SolidColorBrush() {
                    Color = Colors.Transparent
                };
                grid.PointerPressed += (s, e) => {
                    if (type != T.NULL) return;
                    type = T.Move;
                    //e.Handled = true;
                    Debug.WriteLine((s as FrameworkElement).Name);
                };
                grid.Children.Add(Elem<Border>(_ => {
                    _.Name = "对角 等比缩放";
                    _.VerticalAlignment = VerticalAlignment.Top;
                    _.HorizontalAlignment = HorizontalAlignment.Left;
                    _.PointerPressed += (s, e) => {
                        if (type != T.NULL) return;
                        type = T.Resize;
                        //e.Handled = true;
                        Debug.WriteLine((s as FrameworkElement).Name);
                    };
                }));
                grid.Children.Add(Elem<Border>(_ => {
                    _.Name = "对角 等比缩放";
                    _.VerticalAlignment = VerticalAlignment.Top;
                    _.HorizontalAlignment = HorizontalAlignment.Right;
                    _.PointerPressed += (s, e) => {
                        if (type != T.NULL) return;
                        type = T.Resize;
                        //e.Handled = true;
                        Debug.WriteLine((s as FrameworkElement).Name);
                    };
                }));
                grid.Children.Add(Elem<Border>(_ => {
                    _.Name = "对角 等比缩放";
                    _.VerticalAlignment = VerticalAlignment.Bottom;
                    _.HorizontalAlignment = HorizontalAlignment.Left;
                    _.PointerPressed += (s, e) => {
                        if (type != T.NULL) return;
                        type = T.Resize;
                        //e.Handled = true;
                        Debug.WriteLine((s as FrameworkElement).Name);
                    };
                }));
                grid.Children.Add(Elem<Border>(_ => {
                    _.Name = "对角 等比缩放";
                    _.VerticalAlignment = VerticalAlignment.Bottom;
                    _.HorizontalAlignment = HorizontalAlignment.Right;
                    _.PointerPressed += (s, e) => {
                        if (type != T.NULL) return;
                        type = T.Resize;
                        //e.Handled = true;
                        Debug.WriteLine((s as FrameworkElement).Name);
                    };
                }));

                grid.Children.Add(Elem<Border>(_ => {
                    _.Name = "左 翻转"; 
                    _.Child = new Viewbox() {
                        UseLayoutRounding = false,
                        Child = new SymbolIcon() {
                            UseLayoutRounding = false,
                            Margin = new Thickness(5),
                            Symbol = Symbol.Switch,
                        }
                    };
                    _.VerticalAlignment = VerticalAlignment.Center;
                    _.HorizontalAlignment = HorizontalAlignment.Left;
                    _.PointerPressed += (s, e) => {
                        if (type != T.NULL) return;
                        type = T.Flip;
                        //e.Handled = true;
                        Debug.WriteLine((s as FrameworkElement).Name);
                    };
                })); 
                grid.Children.Add(Elem<Border>(_ => {
                    _.Name = "上 旋转";
                    _.Child = new Viewbox() {
                        UseLayoutRounding = false,
                        Child = new SymbolIcon() {
                            UseLayoutRounding = false,
                            Margin = new Thickness(5),
                            Symbol = Symbol.RepeatAll,
                        }
                    };
                    _.CornerRadius = new CornerRadius(100);
                    _.VerticalAlignment = VerticalAlignment.Top;
                    _.HorizontalAlignment = HorizontalAlignment.Center; 
                    _.PointerPressed += (s, e) => {
                        if (type != T.NULL) return;
                        type = T.Rotate;
                        //e.Handled = true;
                        Debug.WriteLine((s as FrameworkElement).Name);
                    };
                }));
                grid.Children.Add(Elem<Line>(_ => {
                    _.Name = "右 水平缩放";
                    _.VerticalAlignment = VerticalAlignment.Center;
                    _.HorizontalAlignment = HorizontalAlignment.Right;
                    _.Y1 = 30; 
                    _.PointerPressed += (s, e) => {
                        if (type != T.NULL) return;
                        type = T.ResizeH;
                        //e.Handled = true;
                        Debug.WriteLine((s as FrameworkElement).Name);

                    };
                }));
                grid.Children.Add(Elem<Line>(_ => {
                    _.Name = "下 垂直缩放";
                    _.VerticalAlignment = VerticalAlignment.Bottom;
                    _.HorizontalAlignment = HorizontalAlignment.Center;
                    _.X1 = 30; 
                    _.PointerPressed += (s, e) => {
                        if (type != T.NULL) return;
                        type = T.ResizeV;
                        //e.Handled = true;
                        Debug.WriteLine((s as FrameworkElement).Name);

                    };
                })); 
                var v = (layer.W+layer.H)/20;
                grid.BorderThickness = new Thickness(0.05 * v);
                foreach (FrameworkElement tcss in grid.Children)
                {
                    switch (tcss)
                    {
                        case Border border when border.Child != null:
                            if (border.HorizontalAlignment == HorizontalAlignment.Center)
                            {
                                border.Width = 1.5 * v;
                                border.Height = 1.5 * v;
                                border.Margin = new Thickness(-2 * v); 
                            }
                            else
                            {
                                border.Width = 0.8 * v;
                                border.Height = 0.8 * v; 
                                border.Margin = new Thickness(-1 * v);
                            }
                            border.BorderThickness = new Thickness(0.05 * v); 
                            border.Background = new SolidColorBrush() {
                                Color = Colors.White
                            };
                            border.BorderBrush = new SolidColorBrush() {
                                Color = Colors.Gray,
                            };
                            break;
                        case Border border:  
                            border.Margin = new Thickness(-0.5 * v);
                            border.Width = 1 * v;
                            border.Height = 1 * v;
                            border.BorderThickness = new Thickness(0.1 * v);
                            border.Background = new SolidColorBrush() {
                                Color = Colors.White
                            };
                            border.BorderBrush = new SolidColorBrush() {
                                Color = Colors.Gray,
                            };
                            break;
                        case Line line:
                            line.Margin = new Thickness(-0.5 * v);
                            line.StrokeThickness = 0.2 * v;
                            line.Stroke = new SolidColorBrush() {
                                Color = Colors.Gray,
                                Opacity = 0.6,
                            };
                            break;
                        default:
                            break;
                    }
                } 
            }); 
        }
    }
}
