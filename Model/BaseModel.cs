using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.Foundation;
using Windows.Devices.Input;
using App2.Model.Tools;
using App2.View;
using System.Diagnostics;

namespace App2.Model
{
    public class BaseModel : DependencyObject 
    {
        public ObservableCollection<LayerModel> LayerList { get; set; } = new ObservableCollection<LayerModel>();
        public ObservableCollection<ToolsModel> ToolsList { get; set; } = new ObservableCollection<ToolsModel>();
        public ObservableCollection<SolidColorBrush> ColorList { get; set; } = new ObservableCollection<SolidColorBrush>();
        public SolidColorBrush MainBrush { get; set; } = new SolidColorBrush(Colors.Black);
        public SolidColorBrush BackBrush { get; set; } = new SolidColorBrush(Colors.AliceBlue);
        public ClipModel Clipper { get; set; } = new ClipModel();

         
        public ObservableCollection<LayerModel> Layers { get => LayerList; set => LayerList = value; }
        //public Windows.UI.Color MainColor
        //{
        //    get { return (Windows.UI.Color)GetValue(MainColorProperty); }
        //    set { if (MainColor == value) return; SetValue(MainColorProperty, value); }
        //}
        //
        //// Using a DependencyProperty as the backing store for MainColor.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty MainColorProperty =
        //    DependencyProperty.Register("MainColor", typeof(Windows.UI.Color), typeof(BaseModel), new PropertyMetadata(Windows.UI.Colors.Azure));
        //
        //
        //
        //public Windows.UI.Color BackColor
        //{
        //    get { return (Windows.UI.Color)GetValue(BackColorProperty); }
        //    set { if (BackColor == value) return; SetValue(BackColorProperty, value); }
        //}
        //
        //// Using a DependencyProperty as the backing store for BackColor.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty BackColorProperty =
        //    DependencyProperty.Register("BackColor", typeof(Windows.UI.Color), typeof(BaseModel), new PropertyMetadata(Windows.UI.Colors.Azure));





        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value);

                Debug.WriteLine("---------------------------");
                Debug.WriteLine(Environment.StackTrace);
                Debug.WriteLine("---------------------------");
            }
        }

        // Using a DependencyProperty as the backing store for Loading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(BaseModel), new PropertyMetadata(false));






        // Using a DependencyProperty as the backing store for SaveLabel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SaveLabelProperty =
            DependencyProperty.Register("SaveLabel", typeof(string), typeof(BaseModel), new PropertyMetadata(""));

        public LayerModel CurrentLayer
        {
            get { return (LayerModel)GetValue(CurrentLayerProperty); }
            set { SetValue(CurrentLayerProperty, value); CurrentTools?.OnLayerChange(DrawPanel.Current);  }
        }

        // Using a DependencyProperty as the backing store for CurrentLayer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentLayerProperty =
            DependencyProperty.Register("CurrentLayer", typeof(LayerModel), typeof(BaseModel), null);



        public ToolsModel LastTools
        {
            get { return (ToolsModel)GetValue(LastToolsProperty); }
            set { SetValue(LastToolsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LastTools.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LastToolsProperty =
            DependencyProperty.Register("LastTools", typeof(ToolsModel), typeof(BaseModel), null);


         
        public ToolsModel CurrentTools
        {
            get { return (ToolsModel)GetValue(CurrentToolsProperty); }
            set
            {
                if (value == null || CurrentTools == value) return;
                //if (value.GetType() != typeof(ResizeModel)) 
                LastTools = CurrentTools;
                CurrentTools?.setState(DrawPanel.Current,false,CurrentLayer);
                SetValue(CurrentToolsProperty, value);
                CurrentTools?.setState(DrawPanel.Current,true,CurrentLayer);
            }
        }

        // Using a DependencyProperty as the backing store for CurrentTools.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentToolsProperty =
            DependencyProperty.Register("CurrentTools", typeof(ToolsModel), typeof(BaseModel), null);


        public Task<StorageFile> CurrentFile => ((Func<Task<StorageFile>>)async delegate {
            if (FileName != "")
            {
                try
                {
                    return await StorageFile.GetFileFromPathAsync(FileName);
                }
                catch (Exception)
                { 
                }
            } 
            var d = DateTime.Now;
            var f = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}.psd", d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);
            var dir = await KnownFolders.PicturesLibrary.CreateFolderAsync("Prpaint", CreationCollisionOption.OpenIfExists);
            var file = await dir.CreateFileAsync(f, CreationCollisionOption.OpenIfExists);

            FileName = file.Path;
            return file;

        })();



        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); Data["FileName"] = value; }
        }

        // Using a DependencyProperty as the backing store for FileName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(BaseModel), new PropertyMetadata(Data["FileName"]??""));




        public Rect DrawRect
        {
            get { return (Rect)GetValue(DrawRectProperty); }
            set { SetValue(DrawRectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DrawRect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DrawRectProperty =
            DependencyProperty.Register("DrawRect", typeof(Rect), typeof(BaseModel), new PropertyMetadata(new Rect()));



        public static IPropertySet Data => ApplicationData.Current.LocalSettings.Values;

        //public string FileName
        //{
        //    get
        //    {
        //        return (Data["FileName"] ?? "").ToString();
        //    }
        //    set
        //    {
        //        Data["FileName"] = value;
        //        SaveLabel =  FileName;
        //    }
        //}
        public string FileWidth
        {
            get => (Data["Width"] ?? "800").ToString();
            set => Data["Width"] = value;
        }
        public string FileHeight
        {
            get => (Data["Height"] ?? "800").ToString();
            set => Data["Height"] = value;
        }








        public PointerDeviceType DrawType
        {
            get {
                return Data["DrawType"] == null ? PointerDeviceType.Touch : PointerDeviceType.Pen;
            }
            set {
                Data["DrawType"] = (value == PointerDeviceType.Touch ? null : "1");
                SetValue(DrawTypeProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for DrawType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DrawTypeProperty =
            DependencyProperty.Register("DrawType", typeof(PointerDeviceType), typeof(BaseModel), new PropertyMetadata(null));



        public bool BarPosition
        {
            get
            {
                return (Data["BarPos"] == null ? false : true);
            }
            set
            {
                Data["BarPos"] = (value == false ? null : "1");
                SetValue(BarPositionProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for BarPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BarPositionProperty =
            DependencyProperty.Register("BarPosition", typeof(bool), typeof(BaseModel), new PropertyMetadata(null));



        public bool PanelReverse
        {
            get
            {
                return (Data["PanelRev"] == null ? false : true);
            }
            set
            {
                Data["PanelRev"] = (value == false ? null : "1");
                SetValue(PanelReverseProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for PanelReverse.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PanelReverseProperty =
            DependencyProperty.Register("PanelReverse", typeof(bool), typeof(BaseModel), new PropertyMetadata(false));



        public bool DrawRotation
        {
            get
            {
                return (Data["DrawRota"] == null ? false : true);
            }
            set
            {
                Data["DrawRota"] = (value == false ? null : "1");
                SetValue(DrawRotationProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for DrawRotation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DrawRotationProperty =
            DependencyProperty.Register("DrawRotation", typeof(bool), typeof(BaseModel), new PropertyMetadata(false));






    }
}
