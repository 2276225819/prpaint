using System;
using System.Collections.Generic;  
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.Storage;
using LayerPaint;
 
namespace System.IO
{
    class BinEx
    {
        protected Dictionary<string, long> t = new Dictionary<string, long>();
        protected static void SwapBytes(ref byte[] b)
        {
            for (long i = 0, nLength = b.Length; i < nLength / 2; ++i)
            {
                byte t = b[i];
                b[i] = b[nLength - i - 1];
                b[nLength - i - 1] = t;
            }
        }
        protected static void SwapBytes(ref sbyte[] b)
        {
            for (long i = 0, nLength = b.Length; i < nLength / 2; ++i)
            {
                sbyte t = b[i];
                b[i] = b[nLength - i - 1];
                b[nLength - i - 1] = t;
            }
        }
        protected long RK(byte len)
        {
            switch (len)
            {
                case 1: return ins.ReadByte();
                case 2: return BitConverter.ToInt16(RD(len), 0);
                case 4: return BitConverter.ToInt32(RD(len), 0);
                case 8: return BitConverter.ToInt64(RD(len), 0);
                default: return 0;
            }

        }
        protected byte[] RD(byte len)
        {
            byte[] b = ins.ReadBytes(len);
            SwapBytes(ref b);
            return b;
        }
        protected void RD(byte len, ref byte val)
        {
            val = ins.ReadByte();
        }
 
        protected void RD(byte len, ref ushort val)
        {
            val = BitConverter.ToUInt16(RD(len), 0);
        }
        protected void RD(byte len, ref uint val)
        {
            val = BitConverter.ToUInt32(RD(len), 0);
        }
        protected void RD(byte len, ref ulong val)
        {
            val = BitConverter.ToUInt64(RD(len), 0);
        }
        protected void RD(byte len, ref string val)
        {
            if (len == 0) return;
            byte[] b = ins.ReadBytes(len);
            for (int i = 0, l = b.Length; i < l; i++)
            {
                val += (char)b[i];
            }
        }
        protected void RD(int len, ref string val)
        { 
            byte[] b = ins.ReadBytes(len);
            for (int i = 0, l = b.Length; i < l; i++)
            {
                val += (char)b[i];
            }
        }
        protected void RD(byte len, ref sbyte val)
        {
            val = ins.ReadSByte();
        }
        protected void RD(byte len, ref short val)
        {
            val = BitConverter.ToInt16(RD(len), 0);
        }
        protected void RD(byte len, ref int val)
        {
            val = BitConverter.ToInt32(RD(len), 0);
        }
        protected void RD(byte len, ref long val)
        {
            val = BitConverter.ToInt64(RD(len), 0);
        }
        protected void RD(byte len, Action skin)
        {
            long pos = ins.BaseStream.Position;
            int l = (int)RK(len);
            skin();
            ins.BaseStream.Position = pos + len + l;
        }
        protected void RD(byte len, Action<int> skin)
        {
            long pos = ins.BaseStream.Position;
            int l = (int)RK(len);
            skin(l);
            ins.BaseStream.Position = pos + len + l;
        }
        protected void RD(byte len, int o)
        {
            ins.BaseStream.Position += len;
        }
        protected BinaryReader ins;
        protected BinaryWriter ous;
        protected void WD(byte len, int o)
        {
            byte[] b = new byte[len];
            ous.Write(b);
        }
        protected void WD(byte len, Action skin)
        {
            int op = (int)ous.BaseStream.Length;
            ous.Write(0);
            skin();
            int l = (int)ous.BaseStream.Length - op-4;
            ous.BaseStream.Position = op;
            byte[] b = BitConverter.GetBytes(l);
            SwapBytes(ref b);
            ous.Write(b);
            ous.BaseStream.Position += l ;
            //ous.Write(new byte[l-ins.BaseStream.Position]); 
        }
        protected void WD(byte len, ref int val)
        {
            byte[] b = BitConverter.GetBytes(val);
            SwapBytes(ref b);
            ous.Write(b);
        }
        protected void WD(byte len, ref short val)
        {
            byte[] b = BitConverter.GetBytes(val);
            SwapBytes(ref b);
            ous.Write(b);
        }
        protected void WD(byte len, ref ushort val)
        {
            byte[] b = BitConverter.GetBytes(val);
            SwapBytes(ref b);
            ous.Write(b);
        }
        protected void WD(byte len, ref string val)
        {
            ous.Write(val.ToCharArray());
        }
        protected void WD(byte len, ref sbyte val)
        {
            ous.Write(val);
        }

        protected void WD(byte len, ref byte val)
        {
            ous.Write(val);
        }
    }

    struct Head
    {
        public short Channels;
        public string Signature;
        public int Height;
        public int Width;
        public short BitsPerPixel;
        public short ColourMode;
        public short Version;
        public Head(int w, int h)
        {
            Width = w;
            Height = h;
            Channels = 3;
            ColourMode = 3;
            Signature = "8BPS";
            BitsPerPixel = 8;
            Version = 1;
        }

    }
    public struct Channel
    {
        public short id;
        public int len;
        public Channel(short i,int size)
        {
            id = i;
            len = size+2;
        }
    }
     class Layer
    {
        public string Signature;
        public string BMK;
        public int Top;
        public int Bottom;
        public int Left;
        public int Right;
        public byte Alpha;
        public string Name;
        public ushort Compression;
        public byte[][] data;
        public byte clipping;
        public byte flags; 
        /*
        public int _top;
        public int _bottom;
        public int _left;
        public int _right;
        public byte _color;
        public byte _flags;
        public byte _parameters;
        public short _padding;
        public byte _realFlags;
        public byte _realBG; 
        */
        public Layer(string name, int offx, int offy, int w, int h, byte alpha, bool show, Stream s)
        {
            var cln = w * h;
            var ln = s.Length;
            Signature = "8BIM";
            BMK = "norm";
            Left = offx;
            Top = offy;
            Right = offx+w;
            Bottom = offy+h;
            Name = name;
            Compression = 0;
            clipping = 0;
            Alpha = alpha;
            flags = (byte)(show ? 8 : 10);
            data = new byte[4][];
            channel = new List<Channel>();
            byte[] b = new byte[ln];
            s.Read(b, 0, (int)ln);
            for (short i =0; i < 4; i++)
            {
                data[i] = new byte[cln];
                channel.Add(new Channel((short)(i - 1), cln));
            }
            for (long i = 0, len = ln / 4; i < len; i++)
            {
                var c = Color.FromBGRA(b, (int)i * 4);
                data[0][i] = c.A;
                data[1][i] = c.R;
                data[2][i] = c.G;
                data[3][i] = c.B;
            }
        }


         /*
        public Layer(string name, int offx, int offy, int w, int h, byte alpha, bool show, Stream s)
        {
            var cln = w * h;
            var ln = s.Length;
            Signature = "8BIM";
            BMK = "norm";
            Left = offx;
            Top = offy;
            Right = offx + w;
            Bottom = offy + h;
            Name = name;
            Compression = 0;
            clipping = 0;
            Alpha = alpha;
            flags = (byte)(show ? 8 : 10);
            data = new byte[4][];
            channel = new List<Channel>();
            byte[] b = new byte[4];
            for (short i = 0; i < 4; i++)
            {
                data[i] = new byte[cln];
                channel.Add(new Channel((short)(i - 1), cln));
            }
            for (long i = 0; s.Position < ln; i++)
            {
                s.Read(b, 0, 4);
                var c = Color.FromBGRA(b);
                data[0][i] = c.A;
                data[1][i] = c.R;
                data[2][i] = c.G;
                data[3][i] = c.B;
            }
        }*/


        public Layer()
        {

        }
        public byte[] getBGRA()
        {
            byte[] b;
            switch (channel.Count)
            {
                case 4:
                    b = new byte[Width * Height * 4];
                    for (int i = 0, l = Width * Height; i < l; i++)
                    {/*/
                    float max = (byte)(data[0][i]) / 255; //这里的255少了一个f导致自动转换成整形计算了
                    b[i * 4 + 0] = (byte)((byte)(data[3][i]) * max);
                    b[i * 4 + 1] = (byte)((byte)(data[2][i]) * max);
                    b[i * 4 + 2] = (byte)((byte)(data[1][i]) * max);
                    b[i * 4 + 3] = (byte)((byte)(data[0][i]));  /*/
                        byte A = data[0][i];
                        byte R = data[1][i];
                        byte G = data[2][i];
                        byte B = data[3][i];
                        float max = (A) / 255f;
                        b[i * 4 + 0] = (byte)(B * max);
                        b[i * 4 + 1] = (byte)(G * max);
                        b[i * 4 + 2] = (byte)(R * max);
                        b[i * 4 + 3] = (byte)(A);

                    }
                    return b;
                case 3:
                    b = new byte[Width * Height * 4];
                    for (int i = 0, l = Width * Height; i < l; i++)
                    {
                        b[i * 4 + 0] = data[2][i];
                        b[i * 4 + 1] = data[1][i];
                        b[i * 4 + 2] = data[0][i];
                        b[i * 4 + 3] = 255;
                    }
                    return b;
                default:
                    return new byte[] { 0, 0, 0, 0 };
            }
        }
         /*
        public int Height
        {
            get { int h = Bottom - Top; return h < 1 ? 1 : h; }
        }
        public int Width
        {
            get { int w = Right - Left; return w < 1 ? 1 : w; }
        }*/
        public int Height
        {
            get { return Bottom - Top;; }
        }
        public int Width
        {
            get  {return Right - Left; }
        }
        public Thickness margin
        {
            get { return new Thickness(Left, Top, 0, 0); }
        }

        public bool Visable {
            get { return flags != 10; }
            set { flags = (byte)(value ? 8 : 10); }
        }

        public List<Channel> channel;

    }
    public struct Res
    {
        public ushort id;
        public string name;
        public string data;
        public void setData(byte[] d)
        {
            data = Convert.ToString(d) ;
        }
 
    }

      class MPSD :BinEx
    {
        public Head head;
        public List<Layer> layer = new List<Layer>();
        public List<Res> res = new List<Res>();
        public string msg
        {
            get;
            set;
        }
        public MPSD()
        {
        }
        /*
        public static MPSD load(string s)
        {
            //stream = new FileStream(path, FileMode.Open); 
            MPSD psd = new MPSD(s); 
            return psd;
        }
        */
        public bool load(Stream stream)
        {
            ins = new BinaryReader(stream);
            //file Header
            RD(4, ref head.Signature);                  //头四字节8BPS
            if (head.Signature != "8BPS")
            {
                msg = "格式出错误";
                return false;
            }
            RD(2, ref head.Version);                    //版本必须等于1
            RD(6, 0);
            RD(2, ref head.Channels);                   //通道数 1-56
            RD(4, ref head.Height);                     //图像高度
            RD(4, ref head.Width);                      //图像宽度
            RD(2, ref head.BitsPerPixel);               //0-9模式 位图/灰阶/索引/RGB/CMYK/多通道/Duotone/Lab
            RD(2, ref head.ColourMode);                 //颜色数据长度
            //color Mode Dar
            RD(4, () => {/*RD(n);*/});
            //image Resources
            RD(4, (len) => {                               //资源 长度 
                int i = (int)ins.BaseStream.Position+len;
                while (ins.BaseStream.Position < len)
                {
                    Res r = new Res();
                    string bim = "";
                    RD(4, ref bim);  //头四字节8BIM
                    if (bim!="8BIM")
                    { 
                        msg = "格式出错误res";
                        return;
                    }
                    // RD(4, 0);                              
                    RD(2, ref r.id);                        //资源ID
                    byte size = RD(1)[0];                   //资源名称 长度
                    size = (byte)((size % 2) == 0 ? 1 : size);
                    RD(size, ref r.name);                   //资源名称 
                    RD(4, (sl) => {                         //资源详细数据 长度
                        RD(sl, ref  r.data);           
                        //r.setData(ins.ReadBytes(sl)) ;         RD(_n, "osType");
                    });
                    if (ins.BaseStream.Position % 2 == 1)   //对齐双字节
                        RD(1, 0);
                    res.Add(r); 
                }
            }); 
            //Layer and Mask Information
            RD(4, (qq) => {                               //图层和蒙版
                RD(4, (ln) => {                           //图层 
                    int h = qq;
                    int count = Math.Abs((int)RK(2));   //图层数
                    try
                    {
                        //bb  = new byte[count][];
                        //Layer records
                        for (int i = 0; i < count; i++)
                        {
                            Layer l; l = new Layer() { channel = new List<Channel>() };
                            RD(4, ref l.Top);               //上边距
                            RD(4, ref l.Left);              //左边距
                            RD(4, ref l.Bottom);            //高
                            RD(4, ref l.Right);             //宽
                            int len = Math.Abs((int)RK(2)); //颜色通道数
                            for (int j = 0; j < len; j++)
                            {
                                Channel c = new Channel();
                                RD(2, ref c.id);            //通道id
                                RD(4, ref c.len);           //通道信息
                                l.channel.Add(c);
                            }
                            RD(4, ref l.Signature);         //头四字节8BIM
                            RD(4, ref l.BMK);               //blendModeKey
                            RD(1, ref l.Alpha);           //透明度
                            RD(1, ref l.clipping);          //边距剪裁
                            RD(1, ref l.flags);             //显示=8 ,隐藏=10
                            RD(1, 0);
                            RD(4, (ll) => {
                                if (ll == 0) return; 
                                //b3 = ins.ReadBytes(ln);  return;
                                RD(4, () => { });
                                RD(4, () => { });
                                byte size = RD(1)[0];
                                RD(size, ref l.Name);
                            });
                            layer.Add(l);
                        }
                    }
                    catch (Exception e)
                    {
                        msg = e.Message;
                        return;
                    } 
                    try
                    {
                                                //Channel image data 
                        for (int i = 0; i < count; i++)
                        {
                            Layer l = layer[i]; l.data = new byte[l.channel.Count][];
                            for (int chn = 0; chn < l.channel.Count; chn++)//颜色通道数据
                            {
                                RD(2, ref l.Compression);                  //通道压缩方式
                                if (l.Compression == 1)
                                    l.data[chn] = rle(l);
                                else
                                    l.data[chn] = raw(l);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        msg = e.Message;
                        return;
                    }
                });
                RD(4, (l) => {                            //Global layer mask info
                    var k = l;
                    //可空 
                });
            });
            //Image Data
            ins.Dispose();
            return msg == null;
        }
        public void save(Stream stream)
        { 
            ous = new BinaryWriter(stream);
            //ous.Write(b2);  ous.Dispose();return;
            //file Header
            WD(4, ref head.Signature);                  //头四字节8BIM
            WD(2, ref head.Version);                    //版本必须等于1
            WD(6, 0);
            WD(2, ref head.Channels);                   //通道数 1-56
            WD(4, ref head.Height);                     //图像高度
            WD(4, ref head.Width);                      //图像宽度
            WD(2, ref head.BitsPerPixel);               //0-9模式 位图/灰阶/索引/RGB/CMYK/多通道/Duotone/Lab
            WD(2, ref head.ColourMode);                 //颜色数据长度
            //color Mode Dar
            WD(4, () => {                               //肯定是空的
                //可空
            });                  
            //image Resources
            WD(4, () => {                               //资源 长度
                //可空 
            });  
            //Layer and Mask Information
            WD(4, () => {                               //图层和蒙版
                WD(4, () => {                            //图层 
                    short _c = (short)layer.Count;
                    WD(2, ref _c);                     //图层数
                    for (int i = 0; i < layer.Count; i++)
                    {
                        Layer l = layer[i];
                        WD(4, ref l.Top);               //上边距
                        WD(4, ref l.Left);              //左边距
                        WD(4, ref l.Bottom);            //高
                        WD(4, ref l.Right);             //宽
                        _c = (short)l.channel.Count;
                        WD(2, ref _c);         //通道数
                        for (int j = 0; j < l.channel.Count; j++)
                        {
                            Channel c = l.channel[j];
                            WD(2, ref c.id);            //通道id
                            WD(4, ref c.len);           //通道信息
                        }
                        WD(4, ref l.Signature);         //头四字节8BIM
                        WD(4, ref l.BMK);               //blendModeKey
                        WD(1, ref l.Alpha);           //透明度
                        WD(1, ref l.clipping);          //边距剪裁
                        WD(1, ref l.flags);             //显示=8 ,隐藏=10
                        WD(1, 0);
                        WD(4, () => {
                            //ous.Write(b3);   return;
                            //WD(4, () => { });
                            //WD(4, () => { });
                            //byte c = (byte)l.Name.Length;
                            //WD(1, ref c);
                            //ous.Write(l.Name);
                        });
                    } 
                    for (int i = 0; i < layer.Count; i++)
                    {
                        Layer l = layer[i]; 
                        for (int chn = 0; chn < l.channel.Count; chn++)//通道数
                        {
                            WD(2, ref l.Compression);
                            ous.Write(l.data[chn]);
                        }
                    } 
                });
                WD(4, () => {  
                    //可空 
                });
                
            });
            //Image Data
            int Compression = 0;
            WD(2, ref Compression);
            ous.Write(new byte[3*head.Width*head.Height]); //这里可以放图标
            ous.Dispose();

        } 
        byte[] raw(Layer l)
        {
            int nWidth = l.Width;
            int nHeight = l.Height;
            int bytesPerPixelPerChannel = head.BitsPerPixel / 8;
            int nPixels = nWidth * nHeight;
            return ins.ReadBytes(nPixels);
        }
        byte[] rle(Layer l)
        {
            byte[] result = new byte[l.Width * l.Height];
            short[] sizeScanLines = new short[l.Height];
            int allSize = 0;
            for (int i = 0; i < l.Height; i++)
            {
                RD(2, ref sizeScanLines[i]);
                allSize += sizeScanLines[i];
            }
            for (int i = 0; i < l.Height; i++)
            {
                byte[] line = unpack(ins.ReadBytes(sizeScanLines[i]), l.Width);
                arraycopy(ref line, 0, ref  result, i * l.Width, line.Length);
            }
            return result;
        }
        public static void arraycopy(ref byte[] a, int s, ref byte[] b, int t, int l)
        {
            for (int i = s; i < l; i++)
                b[i + t] = a[i];
        }
        public byte[] unpack(byte[] data, int size)
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
    }

}

 