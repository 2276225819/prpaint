using LayerPaint;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace App2.Model
{
    public class LayerModel : DependencyObject 
    { 
        public bool IsShow { get => View == 1; set { View = value ? 1.0 : 0.2; Visible = value ? Visibility.Visible : Visibility.Collapsed; } }
        public bool IsEdit { get => Edit == 1; set { Edit = value ? 1.0 : 0.2; } }


        //浪费了两个星期的青春 x:Bind Border.Child + Items.Move 
        //莫名其妙的报错不能用 System.ArgumentException:“Value does not fall within the expected range.”
        public FrameworkElement Child
        {
            get { return App2.View.DrawPanel.getChild(this); }
            set { App2.View.DrawPanel.setChild(this, value);  }
        }

       // Using a DependencyProperty as the backing store for Child.  This enables animation, styling, binding, etc...
       // public static readonly DependencyProperty ChildProperty =
       //     DependencyProperty.Register("Child", typeof(FrameworkElement), typeof(LayerModel), new PropertyMetadata(null));



        public CompositeTransform RenderTransform { get; set; } = new CompositeTransform();

        // // Using a DependencyProperty as the backing store for Composite.  This enables animation, styling, binding, etc...
        // public static readonly DependencyProperty CompositeProperty =
        //     DependencyProperty.Register("Composite", typeof(CompositeTransform), typeof(LayerModel), new PropertyMetadata(new CompositeTransform()));



        // public Model Attr
        // {
        //     get
        //     {
        //         return new Model { B = Bitmap, X = X, Y = Y };
        //     }
        //     set
        //     {
        //         this.Bitmap = value.B;
        //         this.X = value.X;
        //         this.Y = value.Y;
        //     }
        // } 
        // public Rect Rect
        // {
        //     get
        //     {
        //         return new Rect() { X = X, Y = Y, W = Bitmap.PixelWidth, H = Bitmap.PixelHeight };
        //     }
        //     set
        //     {
        //         this.X = value.X;
        //         this.Y = value.Y;
        //         var tmp = new WriteableBitmap(value.W, value.H);
        //         IGrap.copyImg(this.Bitmap, tmp, -value.X, -value.Y);
        //         this.Bitmap = tmp;
        //     }
        // }




        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(LayerModel), new PropertyMetadata(""));



        public WriteableBitmap Bitmap
        {
            get { return (WriteableBitmap)GetValue(BitmapProperty); }
            set { SetValue(BitmapProperty, value); if (value == null) return;
                SetValue(WProperty, (double)value.PixelWidth);
                SetValue(HProperty, (double)value.PixelHeight);  
            }
        }

        // Using a DependencyProperty as the backing store for Bitmap.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BitmapProperty =
            DependencyProperty.Register("Bitmap", typeof(WriteableBitmap), typeof(LayerModel), new PropertyMetadata(null));

        public double View
        {
            get { return (double)GetValue(ViewProperty); }
            set { SetValue(ViewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for View.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewProperty =
            DependencyProperty.Register("View", typeof(double), typeof(LayerModel), new PropertyMetadata(1.0));

        public double Edit
        {
            get { return (double)GetValue(EditProperty); }
            set { SetValue(EditProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Edit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditProperty =
            DependencyProperty.Register("Edit", typeof(double), typeof(LayerModel), new PropertyMetadata(1.0));

        public Visibility Visible
        {
            get { return (Visibility)GetValue(VisibleProperty); }
            set { SetValue(VisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Visible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibleProperty =
            DependencyProperty.Register("Visible", typeof(Visibility), typeof(LayerModel), new PropertyMetadata(Visibility.Visible));



        public double X
        {
            get { return (double)RenderTransform.GetValue(CompositeTransform.TranslateXProperty); }
            set { RenderTransform.SetValue(CompositeTransform.TranslateXProperty, value); }
        }

        // Using a DependencyProperty as the backing store for X.  This enables animation, styling, binding, etc...
        // public static readonly DependencyProperty XProperty =
        //    DependencyProperty.Register("X", typeof(double), typeof(LayerModel), new PropertyMetadata(0d));

        public double Y
        {
            get { return (double)RenderTransform.GetValue(CompositeTransform.TranslateYProperty); }
            set { RenderTransform.SetValue(CompositeTransform.TranslateYProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Y.  This enables animation, styling, binding, etc...
        // public static readonly DependencyProperty YProperty =
        //     DependencyProperty.Register("Y", typeof(double), typeof(LayerModel), new PropertyMetadata(0d));




        public double W
        {
            get { return (double)GetValue(WProperty); }
            set { SetValue(WProperty, value); }
        }

        // Using a DependencyProperty as the backing store for W.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WProperty =
            DependencyProperty.Register("W", typeof(double), typeof(LayerModel), new PropertyMetadata(1.0));
 
        public double H
        {
            get { return (double)GetValue(HProperty); }
            set { SetValue(HProperty, value); }
        }

        // Using a DependencyProperty as the backing store for H.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HProperty =
            DependencyProperty.Register("H", typeof(double), typeof(LayerModel), new PropertyMetadata(1.0));



        public double Opacity
        {
            get { return (double)GetValue(OpacityProperty); }
            set { SetValue(OpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Opacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpacityProperty =
            DependencyProperty.Register("Opacity", typeof(double), typeof(LayerModel), new PropertyMetadata(1.0));
 
        public Point Point
        {
            get { return new Point((int)X,(int) Y); }
            set { X = (int)value.X; Y = (int)value.Y; }
        }


        public void getRect(out Rect r, out WriteableBitmap b)
        {
            r = Bitmap == null ? RectHelper.Empty : new Rect() {
                X = X,
                Y = Y,
                Width = Bitmap.PixelWidth,
                Height = Bitmap.PixelHeight
            };
            b = Bitmap;
        }
        public void setRect(Rect value, WriteableBitmap b, Rect off = default(Rect))
        {
            X = (int)value.X;
            Y = (int)value.Y;
            if (off == default(Rect))
            {
                Bitmap = b; 
            }
            else
            { 
                Bitmap = new WriteableBitmap((int)value.Width, (int)value.Height);
                if (!value.IsEmpty)
                {
                    IGrap.copyImg(b, Bitmap, (int)off.X, (int)off.Y);
                }
            }
        } 
        public void setRect(Rect value )
        {
            if (value == Rect.Empty)
                return;
            X = (int)value.X;
            Y = (int)value.Y;
            W = (int)value.Width;
            H = (int)value.Height; 

        }
    }
}
