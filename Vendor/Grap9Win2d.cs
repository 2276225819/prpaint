using App2;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace App1.Vendor
{
    class Grap9Win2d : Grap9
    {
        public Grap9Win2d()
        {
            device = CanvasDevice.GetSharedDevice();
            rand = new Random();
        }
        Random rand;
        protected CanvasDevice device;
        protected CanvasBlend Blend;
        protected CanvasDrawingSession session;// (Action<CanvasDrawingSession> s)
        protected virtual CanvasBitmap Canvas { get; set; }
        protected virtual CanvasBitmap Shape { get; set; }
        protected virtual CanvasBitmap Texture { get; set; }


        public override void Clear(Color c)
        {
            session.Clear(c);
        }


        public void DrawCircle()
        {
            var s = size_prs ? size * prs : size;
            session.FillCircle(pos, s, color);// dynamicbrush(pos, Vector2.Zero, s)));
        }
        public async void LoadShape(string s)
        {
            Shape = await CanvasBitmap.LoadAsync(device, s);
        }

        int ii = 0;
        TileEffect ECAN;
        Transform2DEffect MAIN  ;
        LuminanceToAlphaEffect MASK;
        ArithmeticCompositeEffect ERAS;
        public override void DrawBrush(Vector2 p, Vector2 v, float s)
        {
            v = Vector2.Normalize(v);
            if (randpos != 0)
            {
                p += new Vector2((float)(rand.NextDouble() - 0.5f) * randpos, (float)(rand.NextDouble() - 0.5f) * randpos);
            }
            if (randsize != 0)
            {
                s += (float)(rand.NextDouble() - 0.5f) * randsize;
                s = Math.Max(0.01f, s);
            }

            var ss = Shape.Size;
            if (randrota == 0)
            {
                MAIN.TransformMatrix = Matrix3x2.Multiply(
                    Matrix3x2.CreateTranslation(p.X - (float)ss.Width / 2, p.Y - (float)ss.Height / 2),
                    Matrix3x2.CreateScale(s / (float)Math.Max(ss.Width, ss.Height), p)
                  );
            }
            else
            {
                var angle = randrota == 1 ? (rand.NextDouble() * 360) : Math.Atan2(v.Y, v.X);
                MAIN.TransformMatrix = Matrix3x2.Multiply(
                    Matrix3x2.CreateTranslation(p.X - (float)ss.Width / 2, p.Y - (float)ss.Height / 2),
                    Matrix3x2.Multiply(
                        Matrix3x2.CreateRotation((float)angle, p),
                        Matrix3x2.CreateScale(s / (float)Math.Max(ss.Width, ss.Height), p)
                    ) 
                );
            }
            //if (randsharp != 0)
            //{
            //    p += new Vector2((float)(rand.NextDouble() - 0.5f) * randsharp, (float)(rand.NextDouble() - 0.5f) * randsharp);
            //}  
             

            if (ECAN != null)
            {
                if (ii++ > 1)
                {
                    ECAN.SourceRectangle = new Rect(p.X + v.X * size  , p.Y + v.Y * size , 1, 1);
                    UpdateCanvas(); ii = 0;
                }
                session.DrawImage(MASK, CanvasComposite.DestinationOut);
                session.DrawImage(ERAS, CanvasComposite.Add);
            }
            else
            {
                session.DrawImage(ERAS);
            }
        }
        public void Loadd()
        {
            MAIN = new Transform2DEffect { Source = Shape, };

            var c = color;
            c.A = (byte)(255 * (density));
            ICanvasImage Main()
            {
                if (Texture == null)
                {
                    return MAIN;
                }
                return new ArithmeticCompositeEffect {
                    Source1 = MAIN,
                    Source2 = new TileEffect {
                        Source = Texture,
                        SourceRectangle = new Rect(0, 0, Texture.Size.Width, Texture.Size.Height)
                    },
                    MultiplyAmount = 1,
                    Source1Amount = 0,
                    Source2Amount = 0,
                };
            }
            ICanvasImage Color()
            {
                var cc = new PremultiplyEffect {
                    Source = new ColorSourceEffect {
                        Color = c
                    }
                };
                if (blend == 0 && dilution == 0)
                {
                    ECAN = null;
                    return cc;
                }
                ECAN = new TileEffect { Source = new GaussianBlurEffect { Source = Canvas, } };
                return new ArithmeticCompositeEffect {
                    Source1 = new CompositeEffect {
                        Sources = {
                                 new PremultiplyEffect{ Source=cc },
                                 new LinearTransferEffect{ Source = ECAN , AlphaSlope = blend }
                             }
                    },
                    MultiplyAmount = 0,
                    Source1Amount = 1 - dilution,
                    Source2 = ECAN,
                    Source2Amount = dilution,
                };
            }
            MASK = new LuminanceToAlphaEffect {
                Source = Main(),

            };
            ERAS = new ArithmeticCompositeEffect {
                Source1 = Color(),
                Source2 = new InvertEffect {
                    Source = new LuminanceToAlphaEffect { Source = Main(), }
                },
                MultiplyAmount = 1,
                Source1Amount = 0,
                Source2Amount = 0,
            };
        }

        public void Loadd2()
        {
            var MAIN = new Transform2DEffect { Source = Shape, };

            //直接读CanvasRenderTarget又不行(0x88990025) sessionblend.Mutiply也没有
            //blendcopy要读底图颜色不过UpdateCanvas太慢了 已经写不出橡皮擦了
            ICanvasImage ccc()
            {
                if (Texture == null)
                {
                    return MAIN;
                }
                return new ArithmeticCompositeEffect {
                    Source1 = MAIN,
                    Source2 = new TileEffect {
                        Source = Texture,
                        SourceRectangle = new Rect(0, 0, Texture.Size.Width, Texture.Size.Height)
                    },
                    MultiplyAmount = 1,
                    Source1Amount = 0,
                    Source2Amount = 0,
                };
            }
            var ECAN = new TileEffect { Source = new GaussianBlurEffect { Source = Canvas, } };
            var ERAS = new CompositeEffect {
                Sources = {
                     new ArithmeticCompositeEffect {
                         Source1 =  new ArithmeticCompositeEffect {
                             MultiplyAmount = 0,
                             Source1 = new CompositeEffect {
                                 Sources = {
                                     new PremultiplyEffect{ Source= new ColorSourceEffect { Color = color} },
                                     new LinearTransferEffect{ Source = ECAN , AlphaSlope = blend }
                                 }
                             },
                             Source1Amount = 1 - dilution,
                             Source2 = ECAN,
                             Source2Amount = dilution,
                         },
                         Source2 = new InvertEffect {
                             Source = new LuminanceToAlphaEffect { Source =  ccc() }
                         } ,
                         MultiplyAmount = 1,
                         Source1Amount = 0,
                         Source2Amount = 0,
                     },
                    new CompositeEffect {
                        Sources = {
                            Canvas,
                           new LuminanceToAlphaEffect {
                                  Source =  ccc()   ,

                         },  },
                        Mode =CanvasComposite.DestinationOut
                    }
                },
                Mode = CanvasComposite.Add
            };
            var brush = new CanvasImageBrush(device) {
                Image = ERAS,
                SourceRectangle = new Rect(new Point(0, 0), Canvas.Size)
            };
            dynamicbrush = (p, v, size) => {
                checkRandom(out Matrix3x2 x, ref p, ref v, ref size);
                ECAN.SourceRectangle = new Rect(p.X + v.X * size, p.Y + v.Y * size, 1, 1);
                MAIN.TransformMatrix = x;
                UpdateCanvas();
                // session.Blend = CanvasBlend.Copy;
                // session.FillCircle(p, size*2, brush);
                session.DrawImage(ERAS, CanvasComposite.Copy);
            };
        }
        ICanvasImage copy99(ICanvasImage img)
        {
            var csize = Shape.Size;
            var sample = new CanvasRenderTarget(device, (float)csize.Width, (float)csize.Height, 96,
                Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Premultiplied
            );
            using (var ss = sample.CreateDrawingSession())
            {
                ss.DrawImage(img);
            }
            return sample;
        }
        void checkRandom(out Matrix3x2 MAIN, ref Vector2 p, ref Vector2 v, ref float size)
        {
            v = Vector2.Normalize(v);
            if (randpos != 0)
            {
                p += new Vector2((float)(rand.NextDouble() - 0.5f) * randpos, (float)(rand.NextDouble() - 0.5f) * randpos);
            }
            if (randsize != 0)
            {
                size += (float)(rand.NextDouble() - 0.5f) * randsize;
                size = Math.Max(0.01f, size);
            }
            var ss = (float)Shape.Size.Width / 2;
            if (randrota == 0)
            {
                MAIN = Matrix3x2.Multiply(
                    Matrix3x2.CreateScale(size / ss),
                    Matrix3x2.CreateTranslation(p.X - size, p.Y - size)
                );
            }
            else
            {
                var angle = randrota == 1 ? (rand.NextDouble() * 360) : Math.Atan2(v.Y, v.X);
                MAIN = Matrix3x2.Multiply(
                    Matrix3x2.Multiply(
                        Matrix3x2.CreateRotation((float)angle, new Vector2(ss, ss)),
                        Matrix3x2.CreateScale(size / ss)
                    ),
                    Matrix3x2.CreateTranslation(p.X - size, p.Y - size)
                );
            }
            //if (randsharp != 0)
            //{
            //    p += new Vector2((float)(rand.NextDouble() - 0.5f) * randsharp, (float)(rand.NextDouble() - 0.5f) * randsharp);
            //} 
        }

        WriteableBitmap b;
        CanvasRenderTarget source;
        private Action<Vector2, Vector2, float> dynamicbrush;

        public override void Init(WriteableBitmap bmp, IGrap9Attr attr = null)
        {
            var newbg = attr.bgurl != bgurl || attr.fgurl != fgurl || attr.hard != hard;
            base.Init(bmp, attr);
            b = bmp;
            source = new CanvasRenderTarget(
                device, b.PixelWidth, b.PixelHeight, 96,
                Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized,
                CanvasAlphaMode.Premultiplied
            );
            Canvas = new CanvasRenderTarget(
                device, b.PixelWidth, b.PixelHeight, 96,
                Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized,
                CanvasAlphaMode.Premultiplied
            );
            Invalidate(true);

            try
            { 
                if (newbg) {  
                    if (fgurl != "")
                    { 
                        var img = LayerPaint.Img.Create(fgurl);
                        Shape = CanvasBitmap.CreateFromBytes(
                            device, img.PixelBuffer, img.PixelWidth, img.PixelHeight,
                            Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized
                            );
                    }
                    else
                    {
                        var px = 200;
                        var csize = Canvas.Size;
                        var sample = new CanvasRenderTarget(device, 2 * px, 2 * px, 96,
                            Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Premultiplied
                        );
                        var tb = new CanvasRadialGradientBrush(device, new[]  {
                            new CanvasGradientStop(){ Color = Color.FromArgb(255,255,255,255),Position=hard },
                            new CanvasGradientStop(){ Color = Color.FromArgb(255,0,0,0),Position=1f },
                        }, CanvasEdgeBehavior.Clamp, CanvasAlphaMode.Straight);
                        using (var s = sample.CreateDrawingSession())
                        {
                            tb.RadiusX = px; tb.RadiusY = px; tb.Center = new Vector2(px, px);
                            s.FillRectangle(0, 0, px * 2, px * 2, tb);
                        }
                        Shape = sample;
                    }
                    if (bgurl != "")
                    {
                        var img = LayerPaint.Img.Create (bgurl);
                        Texture = CanvasBitmap.CreateFromBytes(
                            device, img.PixelBuffer, img.PixelWidth, img.PixelHeight,
                            Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized
                            );
                    }
                    else
                    {
                        Texture = null;
                    }
                    //fgurl = bgurl= ""; 
                }
            }
            catch (Exception e)
            {
                new Windows.UI.Popups.MessageDialog(e.ToString()).ShowMux();
            }
             
            Loadd();
        }
        //CanvasDrawingSession ls;
        public override void Invalidate(bool init = false)
        {
            if (init)
            {
                source.SetPixelBytes(b.PixelBuffer);
            }
            else
            {
                session.Dispose();
                source.GetPixelBytes(b.PixelBuffer);
                b.Invalidate();
            }
            session = source.CreateDrawingSession();
            //session.Blend = Blend;
        }

        protected void UpdateCanvas()
        {
            Canvas.CopyPixelsFromBitmap(source);
        }


        public static async Task<IRandomAccessStream> ff(string s)
        {
            if (s.IndexOf("://") == -1)
            {
                var tmp = await StorageFile.GetFileFromPathAsync(s);
                return await tmp.OpenReadAsync();
            }
            else
            {
                var tmp = await StorageFile.GetFileFromApplicationUriAsync(new Uri(s));
                return await tmp.OpenReadAsync();
            }
        }

    }
}

static class T
{
    public static void DrawImage(this CanvasDrawingSession t, ICanvasImage ii, CanvasComposite c, float o = 1)
    {
        var bb = ii.GetBounds(t.Device);
        t.DrawImage(ii, bb, bb, o, CanvasImageInterpolation.Linear, c);
    }

    public static void Erasing(this CanvasDrawingSession session, ICanvasImage mask, float o = 1)
    {
        session.DrawImage(
            mask, mask.GetBounds(session.Device), mask.GetBounds(session.Device),
            o, CanvasImageInterpolation.Linear, CanvasComposite.DestinationOut
        );
    }
}