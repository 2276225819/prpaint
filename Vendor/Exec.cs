using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

using Windows.System;
namespace LayerPaint
{
    struct Exec
    {
        //static Stack<Exec> undos = new Stack<Exec>();
        static LinkedList<Exec> undos = new LinkedList<Exec>();
        static Stack<Exec> redos = new Stack<Exec>();
        public static bool Undo()
        {
            bool b = undos.Count != 0;
            if (b)
            {
                var t = undos.Last.Value;
                undos.RemoveLast();
                t.undo();
                redos.Push(t); 
            }
            return b;
        }
        public static bool Redo()
        {
            bool b = redos.Count != 0;
            if (b)
            {
                var t = redos.Pop();
                t.exec();
                undos.AddLast(t); 
            }
            return b;
        }
        public static string Count { get { return undos.Count.ToString(); } }
        public static int RedoCount { get { return redos.Count; } }
        public static void Clean() { undos.Clear(); redos.Clear(); }
        public static void CleanRedo() { redos.Clear(); }
        public static void Sep(int len)
        {
            len = Math.Min(undos.Count, len);
            System.Diagnostics.Debug.WriteLine("Gc IN");
            for (int i = 0; i < len; i++)
                undos.RemoveFirst();
            GC.Collect();
            System.Diagnostics.Debug.WriteLine("Gc OUT");
        }
        public static void Do(Exec c)
        {
            c.exec();
            Save(c);
        }
        public static void Save(Exec c)
        {
            undos.AddLast(c);
            if (redos.Count > 0)
                redos.Clear();
            switch (Windows.System.MemoryManager.AppMemoryUsageLevel)
            {
                case AppMemoryUsageLevel.High: Exec.Sep(undos.Count / 2); break;
                case AppMemoryUsageLevel.Medium: Exec.Sep(2); break;
                //case AppMemoryUsageLevel.Low:   break;
                default: break;
            }
            //ulong m = MemoryManager.AppMemoryUsageLimit, u = MemoryManager.AppMemoryUsage;
            //if (u > m * 0.38)  
        }


        public Action exec;
        public Action undo;

    }
}
