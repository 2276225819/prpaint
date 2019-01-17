using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Input;
using Windows.UI.Input;
using System.Collections.ObjectModel;
using App2.Model;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using LayerPaint;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Foundation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Composition;
using System.Numerics;
using Windows.UI;
using Windows.Graphics.Display;
using Windows.UI.Core;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace App2.View
{
    public sealed partial class DrawPanel : UserControl, IModel
    {
        public PointerDeviceType DrawMode { get; set; }
        public AppBar Appbar { get; set; }
        public event Windows.Foundation.TypedEventHandler<object, RoutedEventArgs> OnChangeTools;
        // public event Windows.Foundation.TypedEventHandler<LayerModel, PointerPoint> Drawing;//= (s, e) => { Debug.WriteLine("Drawing"); };
        // public event Windows.Foundation.TypedEventHandler<LayerModel, PointerPoint> DrawBegin;//= (s, e) => { Debug.WriteLine("Drawing"); };
        // public event Windows.Foundation.TypedEventHandler<LayerModel, PointerPoint> DrawCommit;//= (s, e) => { Debug.WriteLine(s.Name); };
        // public event Windows.Foundation.TypedEventHandler<LayerModel, PointerPoint> DrawRollback;//= (s, e) => { Debug.WriteLine(s.Name); };
        public Border ElemArea => GRAPHIC;
        public Action<double> OnChangeDraw { set; get; }
        public ItemsControl ITEMS => LAYS;
        public bool isrota = false;

        public ClipModel Clipper
        {
            get { return (ClipModel)GetValue(ClipperProperty); }
            set { SetValue(ClipperProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Clipper.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClipperProperty =
            DependencyProperty.Register("Clipper", typeof(ClipModel), typeof(DrawPanel), new PropertyMetadata(null));




        public LayerModel CurrentLayer
        {
            get { return VModel.vm.CurrentLayer; }
            set { VModel.vm.CurrentLayer = value; }
        }

        public ToolsModel CurrentTools
        {
            get { return VModel.vm.CurrentTools; }
            set { VModel.vm.CurrentTools = value; }
        }



        public double Scale;



        public Rect DrawRect
        {
            get { return (Rect)GetValue(DrawRectProperty); }
            set
            {
                SetValue(DrawRectProperty, value);
                CANVAS.SetValue(Canvas.WidthProperty, value.Width);
                CANVAS.SetValue(Canvas.HeightProperty, value.Height);
                int ws = 100;
                var ca = ColorHelper.FromArgb(255, 200, 200, 200);
                var cb = ColorHelper.FromArgb(255, 255, 255, 255);
                WriteableBitmap bg = new WriteableBitmap((int)value.Width, (int)value.Height);
                IGrap.fillColor(bg, (x, y) => {
                    return ((x / ws + y / ws) % 2 == 0) ? ca : cb;
                });
                CANVAS.Background = new ImageBrush() { ImageSource = bg };
                isrota = VModel.vm.DrawRotation;
                ResizePanel();
            }
        }

        // Using a DependencyProperty as the backing store for DrawRect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DrawRectProperty =
            DependencyProperty.Register("DrawRect", typeof(Rect), typeof(DrawPanel), new PropertyMetadata(0));



        public void FlipPanel()
        {
            var g = new TransformGroup();
            g.Children.Add(CANVAS.RenderTransform);
            g.Children.Add(new ScaleTransform() {
                ScaleX = -1,
                CenterX = ROOT.ActualWidth / 2,
            });
            CANVAS.RenderTransform = new MatrixTransform() { Matrix = g.Value };

        }
        public async void ResizePanel()
        {
            await Task.Delay(100);
            var value = DrawRect;
            CompositeTransform t = new CompositeTransform();
            await Task.Delay(500);
            var s = Math.Min(ROOT.ActualWidth / value.Width, ROOT.ActualHeight / value.Height);
            t.ScaleX = t.ScaleY = s * 0.94; 
            t.TranslateX = ROOT.ActualWidth / 2 - value.Width / 2;
            t.TranslateY = ROOT.ActualHeight / 2 - value.Height / 2;
            t.CenterX = ROOT.ActualWidth / 2 - t.TranslateX;
            t.CenterY = ROOT.ActualHeight / 2 - t.TranslateY;

            var g = new TransformGroup();
            g.Children.Add(t);
            CANVAS.RenderTransform = new MatrixTransform() { Matrix = g.Value };
            Scale = 1;
        }

        //Border.Child = (UIElement) System.ArgumentException:“Value does not fall within the expected range.”
        public static FrameworkElement getChild(DependencyObject e)
        {
            var a = Current.LAYS.ContainerFromItem(e) as ContentPresenter;
            return (VisualTreeHelper.GetChild(a, 0) as Border)?.Child as FrameworkElement;
        }

        //Border.Child = (UIElement) System.ArgumentException:“Value does not fall within the expected range.”
        public static async void setChild(DependencyObject e, FrameworkElement u)
        {
            var a = Current.LAYS.ContainerFromItem(e) as ContentPresenter;
            while (true)
            {
                var b = VisualTreeHelper.GetChild(a, 0);
                if (b is Border bb)
                {
                    bb.Child = u;
                    break;
                }
                else
                {
                    await Task.Delay(10);
                }
            }
        }


        public ObservableCollection<LayerModel> Layers { get; set; }
        public static DrawPanel Current;
        public DrawPanel()
        {
            Current = this;
            this.InitializeComponent();
            Dispatcher.RunIdleAsync((_) => {
                if (Layers == null) return;
                var loc = false;

                Layers.CollectionChanged += async (s, e) => {
                    if (loc) return;
                    loc = true;
                    await Dispatcher.RunIdleAsync(x => {
                        for (int i = 0; i < LAYS.Items.Count; i++)
                        {
                            ((FrameworkElement)LAYS.ContainerFromIndex(i))?.SetValue(Canvas.ZIndexProperty, LAYS.Items.Count - i);
                        }
                        loc = false;
                    });
                };

                Window.Current.Content.AddHandler(PointerPressedEvent, (PointerEventHandler)delegate (object s, PointerRoutedEventArgs e) {
                    bool? find(FrameworkElement ee)
                    {
                        if (ee?.Name == "ExpandButton")
                            return null;
                        if (ee == null) return false;
                        if (ee == this) return true;
                        return find(VisualTreeHelper.GetParent(ee) as FrameworkElement);
                    }
                    if (find(e.OriginalSource as FrameworkElement) is bool b)
                    {
                        CurrentTools.setState(this, b, CurrentLayer);
                    }
                }, true);

            }).ToString();

            //BUG 丢指针后重置
            //var check = false;
            this.ManipulationCompleted += (s, e) => {
                if (  State == MyEnum.Draw) return;
                    Ev.Clear(); Debug.WriteLine("COMPLETE");
                //check = true;
                //await Task.Delay(300);
                //if (State != MyEnum.Draw)
                //{
                //   
                //}
                //check = false;
            };
        }


        public int CurrentTouchCount { get { return Ev.Count; } }
        HashSet<object> Ev = new HashSet<object>();
        bool IsEraser = false;
        //bool IsInContact = false;
        public enum MyEnum { None, Draw, Move, Stop }
        public MyEnum State = MyEnum.None;
        public double Prs;
        void OnEnter(object sender, PointerRoutedEventArgs e)
        {
            lock (this.Ev)
            {
                if (this.Ev.Add(e.Pointer.PointerId))
                {
                    //Debug.WriteLine("\nOnEnter  " + e.Pointer.PointerId);
                    this.OnMoved(sender, e);
                }
                else
                {
                    Debug.WriteLine("\nOnEnterBoooooooooom");
                }
            }
        }
        void OnExited(object sender, PointerRoutedEventArgs e)
        {
            lock (this.Ev)
            {
                if (this.Ev.Remove(e.Pointer.PointerId))
                {
                    this.OnMoved(sender, e);
                    //Debug.WriteLine("\nOnExited " + e.Pointer.PointerId);
                }
                else
                {
                    Debug.WriteLine("\nOnExitedBoooooooooom");
                }
            }
        }

        bool tsk = false;
        void OnHide()
        {
            GRAPHIC.Visibility = Visibility.Collapsed;
            if (Appbar?.IsOpen == true)
            {
                tsk = true;
                Appbar.IsOpen = false;
            }
        }
        void OnBegin()
        {
            if(isrota != VModel.vm.DrawRotation)
            {
                ResizePanel();
                isrota = VModel.vm.DrawRotation;
            }
        }
        void OnShow()
        {
            GRAPHIC.Visibility = Visibility.Visible;

            //CLIP.StrokeThickness = (1.0 / Scale) * 2;
            if (tsk)
            {
                tsk = false;
                Appbar.IsOpen = true;
            }
        }
        void OnMoved(object sender, PointerRoutedEventArgs e)
        {
            if (CurrentLayer == null || !CurrentLayer.IsEdit || !CurrentLayer.IsShow)
            {
                return;
            }
            var drawable = e.Pointer.IsInContact && (
                (DrawMode == PointerDeviceType.Pen && e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
                || (DrawMode == PointerDeviceType.Touch && this.CurrentTouchCount == 1)
            );
            var pos = e.GetCurrentPoint(LAYS.ItemsPanelRoot);

            Prs = pos.Properties.Pressure;
            switch (State)
            {
                case MyEnum.None:
                    if (pos.Properties.IsRightButtonPressed)
                    {
                        Appbar.IsOpen = !Appbar.IsOpen;
                        State = MyEnum.Stop;
                        return;
                    }
                    if (pos.Properties.IsInverted != IsEraser)
                    {
                        IsEraser = pos.Properties.IsInverted;
                        OnChangeTools?.Invoke(sender, e);
                        return;
                    }
                    if (drawable)
                    {
                        CurrentTools.OnDrawBegin(this, pos);
                        CurrentTools.OnDrawing(this, pos);
                        State = MyEnum.Draw;
                        OnBegin();
                        return;
                    }
                    if(DrawMode == PointerDeviceType.Pen && e.Pointer.PointerDeviceType != PointerDeviceType.Pen)
                    {
                        OnBegin();//bug
                    }
                    //if(DrawMode == PointerDeviceType.Pen && e.Pointer.PointerDeviceType != PointerDeviceType.Pen)
                    //{
                    //    State = MyEnum.Move; ;
                    //    OnBegin();
                    //    OnShow();
                    //}
                    break;
                case MyEnum.Draw:
                    if (drawable)
                    {
                        //i++;
                        CurrentTools.OnDrawing(this, pos);
                    }
                    else
                    {
                        if (!e.Pointer.IsInContact || CurrentTouchCount == 0)
                        {
                            CurrentTools.OnDrawCommit(this, pos);
                            State = MyEnum.None;
                            OnShow();
                        }
                        else
                        {
                            CurrentTools.OnDrawRollback(this, pos);
                            State = MyEnum.Move;
                            OnHide();
                        }
                    }
                    break;
                case MyEnum.Move:
                    if (drawable)
                    {
                        State = MyEnum.Stop;
                    }
                    else
                    if (this.CurrentTouchCount == 0)
                    {
                        State = MyEnum.None;
                        OnShow(); 
                        OnChangeDraw?.Invoke(Scale);
                    }
                    break;
                case MyEnum.Stop:
                    if (!e.Pointer.IsInContact)
                    {
                        State = MyEnum.None;
                        OnShow();
                        OnChangeDraw?.Invoke(Scale);
                    }
                    else
                    if (e.Pointer.IsInContact && !drawable)
                    {
                        State = MyEnum.Move;
                    }
                    break;
                default:
                    break;
            }
        }


        void OnDragDraw(object sender, ManipulationDeltaRoutedEventArgs e)
        { 
            while (true)//switch
            {
                //if(this.DrawMode==PointerDeviceType.Mouse && e.PointerDeviceType == PointerDeviceType.Mouse && e.)break;
                if (this.DrawMode == PointerDeviceType.Touch && this.CurrentTouchCount >= 2) break;
                if (this.DrawMode == PointerDeviceType.Pen && e.PointerDeviceType != PointerDeviceType.Pen) break;
                return;
            }
            var d = e.Delta;
            var t = new TransformGroup();
            t.Children.Add(CANVAS.RenderTransform);
            t.Children.Add(new TranslateTransform() { X = d.Translation.X, Y = d.Translation.Y });

            t.Children.Add(new RotateTransform() { Angle = isrota?d.Rotation:0, CenterX = ROOT.ActualWidth / 2, CenterY = ROOT.ActualHeight / 2 });

            t.Children.Add(new ScaleTransform() { ScaleX = d.Scale, ScaleY = d.Scale, CenterX = ROOT.ActualWidth / 2, CenterY = ROOT.ActualHeight / 2 });
            Scale *= d.Scale;
            CANVAS.RenderTransform = new MatrixTransform() { Matrix = t.Value };
           
        }



        private void ROOT_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var f = e.GetCurrentPoint(null); 
            var t = new CompositeTransform();
            if (e.KeyModifiers == Windows.System.VirtualKeyModifiers.Control)
            {
                t.ScaleY += f.Properties.MouseWheelDelta * 0.001;
                t.ScaleX += f.Properties.MouseWheelDelta * 0.001;
                t.CenterX = f.Position.X;// ROOT.ActualWidth / 2;// - t.TranslateX;
                t.CenterY = f.Position.Y;// ROOT.ActualHeight / 2;// - t.TranslateY;
            }
            else
            {
                if (f.Properties.IsHorizontalMouseWheel)
                {
                    t.TranslateX -= f.Properties.MouseWheelDelta;// * tt.ScaleX;//为什么这样写？？？(水平翻转后坐标转换（竟然忘了 
                    t.CenterX = -ROOT.ActualWidth / 2 - t.TranslateX;
                }
                else
                {
                    t.TranslateY += f.Properties.MouseWheelDelta;
                    t.CenterY = ROOT.ActualHeight / 2 - t.TranslateY;
                }
            }


            var tg = new TransformGroup();
            tg.Children.Add(CANVAS.RenderTransform);
            tg.Children.Add(t);
            CANVAS.RenderTransform = new MatrixTransform() { Matrix = tg.Value };
        }

         
    }
}
