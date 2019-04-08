using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace App2
{
    abstract class Grap9 : IGrap9Attr
    {
        public float size { get; set; }
        public float hard { get; set; }
        public float blend { get; set; }
        public float dilution { get; set; }
        public float persistence { get; set; }
        public float density { get; set; } = 1f;
        public float space { get; set; } = 0.3f;
        public int step { get; set; }
        public bool size_prs { get; set; }
        public bool size_fade { get; set; }
        public int randrota { get; set; }
        public float randpos { get; set; }
        public float randsize { get; set; }
        public float randsharp { get; set; }
        public Color color { get; set; } 
        public string bgurl { get; set; }
        public string fgurl { get; set; }
          
        public abstract void Invalidate(bool init = false);
        public abstract void DrawBrush(Vector2 p, Vector2 v, float s);
        public abstract void Clear(Color c);
        public virtual void Init(WriteableBitmap w,IGrap9Attr attr)
        { 
            if (attr != null)
            {
                size = attr.size;
                hard = attr.hard;
                blend = attr.blend;
                dilution = attr.dilution;
                persistence = attr.persistence;
                density = attr.density;
                space = attr.space;
                step = attr.step;
                size_prs = attr.size_prs;
                size_fade = attr.size_fade;
                randrota = attr.randrota;
                randpos = attr.randpos;
                randsize = attr.randsize;
                randsharp = attr.randsharp;
                color = attr.color;
                bgurl = attr.bgurl;
                fgurl = attr.fgurl; 
            }
        }
        float aa = 0;
        public void MoveTo(float x, float y, float prs = 0.01f)
        {
            //Debug.Write("\n\n\n");
            //Debug.WriteLine($"MoveTo:{x} {y} {prs}");
            if (size_fade)
            {
                aa = 0.5f;
                prs *= aa;
            }
            history.Clear();
            for (int i = 0; i < step; i++)
                history.Add(new Vector3(x, y, prs));
            mm = new Vector2(x, y);
        }
        public void LineTo(float x, float y, float prs)
        {
            //Debug.WriteLine($"LineTo:{x} {y} {prs}");
            if (size_fade)
            {
                if (aa < 2) aa += 0.5f;
                prs *= aa;
            }
            history.Add(new Vector3(x, y, prs));
            if (history.Count > 20)
            {
                history.RemoveAt(0);
            }
        }
       

        float ppp = 1.3f;
        List<Vector3> history = new List<Vector3>();
        protected Vector2 pos { get { if (history.Count < 1) return Vector2.Zero; var a = history[history.Count - 1]; return new Vector2(a.X, a.Y); } }
        Vector2 _pos { get { if (history.Count < 2) return Vector2.Zero; var a = history[history.Count - 2]; return new Vector2(a.X, a.Y); } }
        Vector2 __pos { get { if (history.Count < 3) return Vector2.Zero; var a = history[history.Count - 3]; return new Vector2(a.X, a.Y); } }
        protected float prs { get { if (history.Count < 1) return 0; var a = history[history.Count - 1]; return a.Z; } }
        float _prs { get { if (history.Count < 2) return 0; var a = history[history.Count - 2]; return a.Z; } }
        float __prs { get { if (history.Count < 3) return 0; var a = history[history.Count - 3]; return a.Z; } }


        float Lerp(float b, float a, float f) { return a * f + (b * (1 - f)); }
       
        Vector2 mm = Vector2.Zero;
        //public void _DrawBerzier()
        //{
        //    if (history.Count < 3) return;
        //    var __pos = history[history.Count - 3];
        //    var _pos = history[history.Count - 2];
        //    var pos = history[history.Count - 1];
        //    var pa = Vector3.Lerp(__pos, _pos, 0.5f);// Vec2.getCen(p0, p1);
        //    var pb = Vector3.Lerp(_pos, pos, 0.5f);// Vec2.getCen(p1, p2);
        //    var sn = (float)((__pos - _pos).Length() + (_pos - pos).Length());
        //    float sep = 1 / (sn * 1f);
        //    for (float i = 0f; i < 1f; i += sep)
        //    {
        //        var m3 = this.Berzier(new[] { pa, _pos, pb }, i, Vector3.Lerp);
        //        var m = new Vector2(m3.X, m3.Y);
        //        var tmp = (m - mm).Length();
        //        var s = size_prs ? size * m3.Z : size;
        //        float weight = s * space;
        //        weight = weight < ppp ? ppp : weight;
        //        if (tmp > weight)
        //        {
        //            var om = m;
        //            DrawBrush(m, om - mm, s);
        //            mm = om;
        //        }
        //    }
        //}
        public void DrawBerzier()
        {
            if (history.Count < step + 2) { return; }

            var p1 = history.GetRange(history.Count - step - 2, step).ToArray();
            var p2 = history.GetRange(history.Count - step - 1, step).ToArray();
            var p3 = history.GetRange(history.Count - step - 0, step).ToArray();
            var __p = p1.Length == 1 ? p1[0] : Berzier(p1, 0.5f, Vector3.Lerp);
            var _p = p2.Length == 1 ? p2[0] : Berzier(p2, 0.5f, Vector3.Lerp);
            var p = p3.Length == 1 ? p3[0] : Berzier(p3, 0.5f, Vector3.Lerp);
            var pa = Vector3.Lerp(__p, _p, 0.5f);// Vec2.getCen(p0, p1);
            var pb = Vector3.Lerp(_p, p, 0.5f);// Vec2.getCen(p1, p2);
            var sn = (float)((__p - _p).Length() + (_p - p).Length());
            float sep = 1 / (sn * 1f);
            for (float i = 0f; i < 1f; i += sep)
            {
                var m3 = this.Berzier(new[] { pa, _p, pb }, i, Vector3.Lerp);
                var m = new Vector2(m3.X, m3.Y);
                var tmp = (m - mm).Length();
                var s = size_prs || size_fade ? size * m3.Z : size;
                float weight = s * space;
                weight = weight < ppp ? ppp : weight;
                if (tmp > weight)
                {
                    DrawBrush(m, m - mm, s);
                    mm = m;
                }
            }
        }
        public void DrawEnd(bool end)
        {
            if (history.Count < step + 2) { return; }
            var pp = pos;// + (pos - _pos) / 4;

            if (1 < step)
            {
                var p2 = history.GetRange(history.Count - step - 1, step).ToArray();
                var p3 = history.GetRange(history.Count - step - 0, step).ToArray();
                var pa = Berzier(p2, 0.5f, Vector3.Lerp);
                var pb = Berzier(p3, 0.5f, Vector3.Lerp); 
                var pc = history[history.Count - 1];
                if(size_fade) pc.Z =   -pb.Z*0.75f ; 
                history.Clear();
                history.Add(pa);
                history.Add(pb);
                history.Add(pc); 

                var s = step; 
                step = 1; 
                DrawBerzier();
                step = s; 
            }
            //else
            //{
            //    var pb = pos + (_pos - pos) / 2;
            //    var pr = prs + (_prs - prs) / 2;
            //    LineTo(pb.X, pb.Y, pr);
            //    LineTo(pp.X, pp.Y, pr / 4);
            //    DrawLine();
            //} 
        }
        public void DrawLine()
        {
            if (history.Count < 2) return;
            float sep = 0;
            for (float t = 0; t < 1; t += sep)
            {
                var m3 = Vector3.Lerp(history[history.Count - 2], history[history.Count - 1], t);
                var m = new Vector2(m3.X, m3.Y);
                var s = Math.Max(0.01f, size_prs ? size * m3.Z : size);
                DrawBrush(m, pos - _pos, s);
                var weight = s * space;
                weight = weight < ppp ? ppp : weight;
                sep = weight / ((pos - _pos).Length());
            }
        }

        T Berzier<T>(T[] pos, float t, Func<T, T, float, T> Lerp)
        {
            if (pos.Length == 2)
            {
                return Lerp(pos[0], pos[1], t);
            }
            T[] newPos = new T[pos.Length - 1];
            for (int i = 0; i < pos.Length - 1; i++)
            {
                newPos[i] = Lerp(pos[i], pos[i + 1], t);
            }
            return Berzier(newPos, t, Lerp);
        }
    }

    interface IGrap9Attr
    {
        float size { get; set; }
        float hard { get; set; }
        float density { get; set; }
        float blend { get; set; }
        float dilution { get; set; }
        float persistence { get; set; }
        float space { get; set; }
        int step { get; set; }
        bool size_prs { get; set; }
        bool size_fade { get; set; }
        int randrota { get; set; }
        float randpos { get; set; }
        float randsize { get; set; }
        float randsharp { get; set; }
        Color color { get; set; }
         
        string bgurl { get; set; }
        string fgurl { get; set; }
    }

}
