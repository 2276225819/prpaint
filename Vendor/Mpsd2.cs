using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{
    static class TYPE
    {
        public static T Get<T>(this List<T> t, int index) where T : new()
        {
            while (t.Count <= index) t.Add(new T());
            return t[index];
        }
    }
    class Mpsd2 
    {
        public Encoding encode;
        public Head head = new Head();
        public short layer;
        public List<Layer> layerdata = new List<Layer>(); 
        public bool load(Stream s)
        { 
            var r = new R() { encoding = encode } ;
            r.BaseStream = s;
            r.format(this);
            foreach (var item in layerdata)
            {
                item.DecodeRaw(head.width, head.height);
            }
            s.Dispose();
            return true;
        }
        public bool save(Stream s)
        { 
            var w = new W() { encoding = encode };
            w.BaseStream = s;
            layer = (short)layerdata.Count;
            //foreach (var item in layerdata)
            //{
            //    item.EncoRaw(head.width, head.height);
            //}
            w.format(this);
            s.Dispose();
            return true;
        }

        public class Head
        {
            public int height;
            public int width;
            public string Signature;
            public short BitsPerPixel;
            public short ColourMode;
            public short Version;
            public short channel;
            public static Head Create(int w, int h)
            {
                return new Head() {
                    width = w,
                    height = h,
                    ColourMode = 3,
                    Signature = "8BPS",
                    BitsPerPixel = 8,
                    Version = 1,
                    channel = 3,
                };
            }
        }
        public class Layer
        {
            public string Signature;
            public string BMK;
            public int Top;
            public int Bottom;
            public int Left;
            public int Right;
            public byte Alpha;
            public string Name;
            public byte clipping;
            public byte flags;



            public short channel;
            public List<Channel> channeldata = new List<Channel>();
            public class Channel
            {
                public short id;
                public int len;
                public ushort compression;
                public byte[] data;
            }

            public int Height
            {
                get { return Bottom - Top; ; }
            }
            public int Width
            {
                get { return Right - Left; }
            }
            public bool Visable
            {
                get { return (flags & 0b10) == 0; }
                //set { flags = (byte)(value ? 8 : 10); }
            }
            public bool Editable
            {
                get { return (flags & 0b1) == 0; }
                //set { flags = (byte)(value ? 8 : 10); }
            }
            public static System.Threading.Tasks.Task<Layer> CreateAsync(string name, int offx, int offy, int w, int h, byte alpha, bool show,bool edit, Stream s)
            {
                return System.Threading.Tasks.Task.Run(() => Create(name, offx, offy, w, h, alpha, show,edit, s));
            }
            public static Layer Create(string name, int offx, int offy, int w, int h, byte alpha, bool show,bool edit, Stream s)
            {
                var l = new Layer();
                var cln = w * h;
                var ln = s.Length;
                l.Signature = "8BIM";
                l.BMK = "norm";
                l.Left = offx;
                l.Top = offy;
                l.Right = offx + w;
                l.Bottom = offy + h;
                l.Name = name;
                l.clipping = 0;
                l.Alpha = alpha;
                l.flags = (byte)(0b1000 | (show ? 0b0 : 0b10) | (edit ? 0b0 : 0b1));
                byte[] b = new byte[ln];//已预乘
                s.Read(b, 0, (int)ln);
                l.channel = 4;
                for (short i = 0; i < 4; i++)
                {
                    var c = l.channeldata.Get(i);
                    c.compression = 0;
                    c.data = new byte[cln];
                    c.id = (short)(i - 1);
                    c.len = cln + 2;
                }
                for (long i = 0, len = ln / 4; i < len; i++)
                {
                    double _a = b[i * 4 + 3] / 255.0;
                    l.channeldata[0].data[i] = b[i * 4 + 3];
                    l.channeldata[1].data[i] = (byte)Math.Round(b[i * 4 + 2] / _a);
                    l.channeldata[2].data[i] = (byte)Math.Round(b[i * 4 + 1] / _a);
                    l.channeldata[3].data[i] = (byte)Math.Round(b[i * 4 + 0] / _a);
                }
                return l;
            }
            public byte[] getBGRA(int w, int h)
            {
                byte[] b = new byte[w * h * 4];
                switch (channel)
                {
                    case 4:
                        for (int i = 0, l = w * h; i < l; i++)
                        {
                            double _a =  channeldata[0].data[i] / 255.0 ;
                            b[i * 4 + 0] = (byte)Math.Round(channeldata[3].data[i] * _a);
                            b[i * 4 + 1] = (byte)Math.Round(channeldata[2].data[i] * _a);
                            b[i * 4 + 2] = (byte)Math.Round(channeldata[1].data[i] * _a);
                            b[i * 4 + 3] = (byte)(channeldata[0].data[i]);

                        }
                        return b;
                    case 3:
                        for (int i = 0, l = w * h; i < l; i++)
                        {
                            b[i * 4 + 0] = channeldata[2].data[i];
                            b[i * 4 + 1] = channeldata[1].data[i];
                            b[i * 4 + 2] = channeldata[0].data[i];
                            b[i * 4 + 3] = 255;
                        }
                        return b;
                    default:
                        throw new Exception("getBGRA");
                }
            }


            byte[] rle(byte[] s, int w, int h)
            {
                BinaryReader ins = new BinaryReader(new MemoryStream(s));
                byte[] result = new byte[w * h];
                short[] sizeScanLines = new short[h];
                int allSize = 0;
                for (int i = 0; i < h; i++)
                {
                    sizeScanLines[i] = BitConverter.ToInt16(ins.ReadBytes(2).Reverse().ToArray(), 0);
                    allSize += sizeScanLines[i];
                }
                for (int i = 0; i < h; i++)
                {
                    byte[] line = unpack(ins.ReadBytes(sizeScanLines[i]), w);
                    arraycopy(ref line, 0, ref result, i * w, line.Length);
                }
                return result;
            }
            static void arraycopy(ref byte[] a, int s, ref byte[] b, int t, int l)
            {
                for (int i = s; i < l; i++)
                    b[i + t] = a[i];
            }
            byte[] unpack(byte[] data, int size)
            {
                byte[] result = new byte[size];
                int writePos = 0;
                int readPos = 0;
                while (readPos < data.Length)
                {
                    int n = (sbyte)data[readPos++];
                    if (n > 0)
                    {
                        int count = n + 1;
                        for (int j = 0; j < count; j++)
                            result[writePos++] = data[readPos++];
                    }
                    else
                    {
                        byte b = data[readPos++];
                        int count = -n + 1;
                        for (int j = 0; j < count; j++)
                            result[writePos++] = b;
                    }
                }
                return result;
            }
            public void DecodeRaw(int w, int h)
            {
                foreach (Channel item in channeldata)
                {
                    switch (item.compression)
                    {
                        case 0:
                            break;
                        case 1://rle
                            item.data = rle(item.data, Width, Height);
                            item.compression = 0;
                            item.len = item.data.Length + 2;
                            break;
                        default:
                            throw new Exception("DecodeRaw");
                    }
                }
            }
        }
        // public List<Res> res = new List<Res>();
        // public struct Res
        // {
        //     public ushort id;
        //     public string name;
        //     public string data; 
        // }
        abstract class Base
        {
            public void format(Mpsd2 m)
            {
                S(4, ref m.head.Signature);                  //头四字节8BPS
                if (m.head.Signature != "8BPS")
                    throw new Exception("8BPS");
                D(2, ref m.head.Version);                    //版本必须等于1
                S(6);
                D(2, ref m.head.channel);                    //通道数 1-56
                D(4, ref m.head.height);                    //图像高度
                D(4, ref m.head.width);                     //图像宽度
                D(2, ref m.head.BitsPerPixel);               //DepthNumber  1 8 16 32
                D(2, ref m.head.ColourMode);                 //颜色数据长度 0位图/1灰阶/2索引/3RGB/4CMYK/7多通道/8Duotone/9Lab

                //color Mode Dar
                B(4, (n) => {
                    S(n);
                });
                //image Resources
                B(4, (n) => {                               //资源 长度 
                    S(n);
                    //int i = (int)ins.BaseStream.Position + len;
                    //while (ins.BaseStream.Position < len)
                    //{
                    //    Res r = new Res();
                    //    string bim = "";
                    //    RD(4, ref bim);  //头四字节8BIM
                    //    if (bim != "8BIM")
                    //    {
                    //        msg = "格式出错误res";
                    //        return;
                    //    }
                    //    // RD(4, 0);                              
                    //    RD(2, ref r.id);                        //资源ID
                    //    byte size = RD(1)[0];                   //资源名称 长度
                    //    size = (byte)((size % 2) == 0 ? 1 : size);
                    //    RD(size, ref r.name);                   //资源名称 
                    //    RD(4, (sl) => {                         //资源详细数据 长度
                    //        RD(sl, ref r.data);
                    //        //r.setData(ins.ReadBytes(sl)) ;         RD(_n, "osType");
                    //    });
                    //    if (ins.BaseStream.Position % 2 == 1)   //对齐双字节
                    //        RD(1, 0);
                    //    res.Add(r);
                    //}
                });
                //Layer and Mask Information
                B(4, (n) => {                                   //图层和蒙版
                    var a = BaseStream.Position;
                    //Layer info 
                    B(4, (nn) => {                              //图层     
                        var aa = BaseStream.Position;
                        D(2, ref m.layer);                      //图层数
                        if (m.layer < 0) m.layer *= -1;         //mergedAlpha  
                        //Layer records
                        for (int i = 0; i < m.layer; i++)
                        {
                            var l = m.layerdata.Get(i);
                            D(4, ref l.Top);               //上边距
                            D(4, ref l.Left);              //左边距
                            D(4, ref l.Bottom);            //高
                            D(4, ref l.Right);             //宽 
                            D(2, ref l.channel);            //颜色通道数 
                            for (int j = 0; j < l.channel; j++)
                            {
                                var cc = l.channeldata.Get(j);
                                D(2, ref cc.id);            //通道id
                                D(4, ref cc.len);           //通道信息
                            }
                            S(4, ref l.Signature);         //头四字节8BIM 
                            if (l.Signature != "8BIM")
                                throw new Exception("8BIM");
                            S(4, ref l.BMK);               //Blend Mode Key 
                            D(1, ref l.Alpha);           //透明度
                            D(1, ref l.clipping);          //边距剪裁
                            D(1, ref l.flags);             //显示=8 ,隐藏=10
                            S(1);
                            B(4, (nnn) => {
                                var aaa = BaseStream.Position;
                                //Layer mask
                                B(4, (nnnn) => {
                                    S(nnnn);
                                });
                                //Layer blending ranges data
                                B(4, (nnnn) => {
                                    S(nnnn);
                                });
                                //Pascal strings padding 4
                                P(4, ref l.Name);
                                //Additional Layer Information
                                addition(aaa, nnn);
                            });
                        }
                        //Channel image data 
                        for (int i = 0; i < m.layer; i++)
                        {
                            var l = m.layerdata.Get(i);
                            //Layer l = m.layer[i]; l.data = new byte[l.channel.Count][];
                            for (int chn = 0; chn < l.channel; chn++)//颜色通道数据
                            {
                                var cc = l.channeldata.Get(chn);
                                D(2, ref cc.compression);                  //通道压缩方式  
                                S(cc.len - 2, ref cc.data);
                            }
                        }
                        addition(aa, nn);
                    });
                    //Global layer mask info
                    B(4, (nn) => {
                        S(n);
                        //var k = l;
                        //可空 
                    });
                    //Additional Layer Information
                    void addition(long aa, int nn)
                    {
                        var nnn = (int)(BaseStream.Position - aa);
                        //if (nn > nnn)
                        //{
                        //    var str = "";
                        //    S(4, ref str);         // 四字节8BIM 
                        //    if (str != "8BIM")
                        //    { 
                        //        throw new Exception("8BIM");
                        //    }
                        //        
                        //
                        //}
                        var kk = new byte[0];
                        S(Math.Max(0, nn - nnn), ref kk);
                    }
                    addition(a, n);
                });
                //Image Data 
                ushort compression = 0;
                D(2, ref compression);
                //if (compression != 0) throw null;
                byte[] img = new byte[m.head.height * m.head.width * m.head.channel];
                S(img.Length, ref img);
                //D(2, ref m.image.channeldata.Get(0).compression);
                //for (int chn = 0; chn < m.image.channel; chn++)
                //{
                //    var c = m.image.channeldata.Get(chn);
                //    S(c.len - 2, ref c.dara);
                //}
            }


            public Encoding encoding;
            public Stream BaseStream;
            public byte[] Read(int l, bool r = false)
            {
                var b = new byte[l];
                BaseStream.Read(b, 0, l);
                return r ? b : b.Reverse().ToArray();
            }
            public void Write(byte[] l, bool r = false)
            {
                BaseStream.Write(r ? l : l.Reverse().ToArray(), 0, l.Length);
            }

            // D 1byte  2short  4int 8long 1ubyte 2ushort  4uint  8ulong R
            // S nSkip  Action
            // P nString padding
            public abstract void D(int i, ref sbyte v, bool r = false);
            public abstract void D(int i, ref short v, bool r = false);
            public abstract void D(int i, ref int v, bool r = false);
            public abstract void D(int i, ref long v, bool r = false);

            public abstract void D(int i, ref byte v, bool r = false);
            public abstract void D(int i, ref ushort v, bool r = false);
            public abstract void D(int i, ref uint v, bool r = false);
            public abstract void D(int i, ref ulong v, bool r = false);

            public abstract void B(int i, Action<int> sg);
            public abstract void P(int i, ref string v);
            public abstract void S(int i, ref string v);
            public abstract void S(int i, ref byte[] v);
            public abstract void S(int i);
        }
        class R : Base
        {
            public override void B(int i, Action<int> skin)
            {
                long pos = BaseStream.Position;
                var b = 0;
                switch (i)
                {
                    case 1: b = BaseStream.ReadByte(); break;
                    case 2: b = BitConverter.ToInt16(Read(i), 0); break;
                    case 4: b = BitConverter.ToInt32(Read(i), 0); break;
                    //case 8: b = BitConverter.ToInt64(Read(len), 0); break;
                    default: throw new Exception("RB");
                }
                if (b > 0)
                    skin(b);
                if (BaseStream.Position != pos + b + i)
                    throw new Exception("RBK");

            }
            public override void S(int i)
            {
                Read(i);
            }
            public override void S(int i, ref string v)
            {
                var c = Read(i, true);
                v = encoding.GetString(c);
            }
            public override void S(int i, ref byte[] v)
            {
                v = Read(i, true);
            }
            public override void P(int padding, ref string v)
            {
                byte len = 0;
                D(1, ref len);
                len = (byte)(((len / padding) + 1) * 4 - 1);
                S(len, ref v);
            }

            public override void D(int i, ref sbyte v, bool r = false) { var b = Read(1)[0]; v = (sbyte)(b > 127 ? b - 256 : b); }
            public override void D(int i, ref short v, bool r = false) { v = (short)BitConverter.ToInt16(Read(i, r), 0); }
            public override void D(int i, ref int v, bool r = false) { v = (int)BitConverter.ToInt32(Read(i, r), 0); }
            public override void D(int i, ref long v, bool r = false) { v = (long)BitConverter.ToInt64(Read(i, r), 0); }
            public override void D(int i, ref byte v, bool r = false) { v = Read(i)[0]; }
            public override void D(int i, ref ushort v, bool r = false) { v = (ushort)BitConverter.ToUInt16(Read(i, r), 0); }
            public override void D(int i, ref uint v, bool r = false) { v = (uint)BitConverter.ToUInt32(Read(i, r), 0); }
            public override void D(int i, ref ulong v, bool r = false) { v = (ulong)BitConverter.ToUInt64(Read(i, r), 0); }
            // public override void D<V>(int i, ref V v, bool r = false)
            // {
            //     switch (v)
            //     {
            //         case sbyte i8 when i == 1:
            //             var b = Read(1)[0];
            //             v = (V)(object)(b > 127 ? b - 256 : b);
            //             break;
            //         case short i16 when i == 2:
            //             v = (V)(object)BitConverter.ToInt16(Read(i, r), 0);
            //             break;
            //         case int i32 when i == 4:
            //             v = (V)(object)BitConverter.ToInt32(Read(i, r), 0);
            //             break;
            //         case long i64 when i == 8:
            //             v = (V)(object)BitConverter.ToInt64(Read(i, r), 0);
            //             break;
            //         case byte ui8 when i == 1:
            //             v = (V)(object)Read(i)[0];
            //             break;
            //         case ushort ui16 when i == 2:
            //             v = (V)(object)BitConverter.ToUInt16(Read(i, r), 0);
            //             break;
            //         case uint ui32 when i == 4:
            //             v = (V)(object)BitConverter.ToUInt32(Read(i, r), 0);
            //             break;
            //         case ulong ui64 when i == 8:
            //             v = (V)(object)BitConverter.ToUInt64(Read(i, r), 0);
            //             break;
            //         default: throw null;
            //     }
            // }
        }
        class W : Base
        {
            public override void B(int i, Action<int> skin)
            {
                int op = (int)BaseStream.Position;
                Write(new byte[i]);
                skin(0);
                var l = BaseStream.Position - op - 4;
                BaseStream.Position = op;
                switch (i)
                {
                    case 1: Write(BitConverter.GetBytes((byte)l)); break;
                    case 2: Write(BitConverter.GetBytes((short)l)); break;
                    case 4: Write(BitConverter.GetBytes((int)l)); break;
                    //case 8: b = BitConverter.ToInt64(Read(len), 0); break;
                    default: throw new Exception("WB");
                }
                BaseStream.Position += l;
            }

            public override void D(int i, ref sbyte v, bool r = false) { Write(new byte[] { (byte)v }); }
            public override void D(int i, ref short v, bool r = false) { Write(BitConverter.GetBytes(v)); }
            public override void D(int i, ref int v, bool r = false) { Write(BitConverter.GetBytes(v)); }
            public override void D(int i, ref long v, bool r = false) { Write(BitConverter.GetBytes(v)); }
            public override void D(int i, ref byte v, bool r = false) { Write(new byte[] { (byte)v }); }
            public override void D(int i, ref ushort v, bool r = false) { Write(BitConverter.GetBytes(v)); }
            public override void D(int i, ref uint v, bool r = false) { Write(BitConverter.GetBytes(v)); }
            public override void D(int i, ref ulong v, bool r = false) { Write(BitConverter.GetBytes(v)); }
            // public override void D<V>(int i, ref V v, bool r = false)
            // {
            //     switch (v)
            //     {
            //         case sbyte i8 when i == 1:
            //             Write(new byte[] { (byte)i8 });
            //             break;
            //         case short i16 when i == 2:
            //             Write(BitConverter.GetBytes(i16));
            //             break;
            //         case int i32 when i == 4:
            //             Write(BitConverter.GetBytes(i32));
            //             break;
            //         case long i64 when i == 8:
            //             Write(BitConverter.GetBytes(i64));
            //             break;
            //         case byte ui8 when i == 1:
            //             Write(new byte[] { (byte)ui8 });
            //             break;
            //         case ushort ui16 when i == 2:
            //             Write(BitConverter.GetBytes(ui16));
            //             break;
            //         case uint ui32 when i == 4:
            //             Write(BitConverter.GetBytes(ui32));
            //             break;
            //         case ulong ui64 when i == 8:
            //             Write(BitConverter.GetBytes(ui64));
            //             break;
            //         default: throw null;
            //     }
            // }
            public override void P(int padding, ref string v)
            {
                if (v == null)
                    v = "";
                var ss = encoding.GetBytes(v);
                byte len = (byte)((( (byte)ss.Length / padding) + 1) * 4 - 1);
                //v = v.PadRight(len);
                D(1, ref len);
                S(len, ref ss); 
            }

            public override void S(int i)
            {
                Write(new byte[i]);
            }
            public override void S(int i, ref string v)
            {
                Write(encoding.GetBytes(v), true);
            }
            public override void S(int i, ref byte[] v)
            {
                var b = new byte[i];
                v.CopyTo(b, 0);
                Write(b, true);
            }
        }
    }
}
