using App2.Model.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace App2.Model
{
    public class ToolsModel : DependencyObject
    {
        public static ClipModel Clipper;
        public static Rect DrawRect => MainPage.Current.DrawRect;
        public virtual Color Color => MainPage.Current.MainColor;

        public virtual string Icon { get; set; }

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(ToolsModel), new PropertyMetadata(""));


        bool state = false;
        public void setState(IModel m, bool value,LayerModel CurrentLayer)
        {
            if (CurrentLayer == null)
            {
                return;
            }
            if (state != value)
            {
                OnToolState(m, state = value);
            }

        }

        public ToolsModel()
        {

        }
        public ToolsModel(string[] arr, out int i)
        {
            i = 1;
            Name = arr[i++];
        }
        public virtual string ToData()
        {
            return ToString().Substring(ToString().LastIndexOf('.')+1) + "|" + Name;
        }
        public static ToolsModel Create(string s)
        {
            var arr = s.Split('|');
            switch (arr[0])
            {
                case "PenModel":
                    return new PenModel(arr, out var i1);
                
                case "EraserModel":
                    return new EraserModel(arr, out var i2); 
            }
            return null;
        } 

        public virtual void OnDrawing(IModel sender, PointerPoint args)
        { 
        }
        public virtual void OnDrawBegin(IModel sender, PointerPoint args)
        { 
        }

        public virtual void OnDrawCommit(IModel sender, PointerPoint args)
        { 
        }
        public virtual void OnDrawRollback(IModel sender, PointerPoint args)
        { 
        }

        public virtual void OnToolState(IModel sender,bool state)
        {
        }

        public virtual void OnLayerChange(IModel sender)
        {
        }

        public T Elem<T>(Action<T> cb) where T : DependencyObject, new()
        {
            var tcss = new T();
            cb(tcss);
            return tcss;
        }
    }

    public interface IModel
    {
        LayerModel CurrentLayer { get; set; }
        ObservableCollection<LayerModel> Layers { get; set; }
        Action<double> OnChangeDraw { set; }
        Border ElemArea { get; }
        
    }
}
