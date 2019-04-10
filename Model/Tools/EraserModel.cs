using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using Windows.UI;

namespace App2.Model.Tools
{
    class EraserModel : PenModel
    {

        public EraserModel() : base()   { }
        public EraserModel(string[] arr) : base(arr) { }


        public override string Icon { get; set; } = "ms-appx:///Assets/AppBar/eraser.png";
        public override Color Color => Colors.Transparent;

        public override void OnDrawBegin(IModel sender, PointerPoint args)
        {
            if (Clipper.IsCliping)
            {
                layer.Child = null;//不支持
            }
            base.OnDrawBegin(sender, args);
        }


    }
}
