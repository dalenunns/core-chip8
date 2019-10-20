using GLib;
using SkiaSharp;
using Gtk;
using System;

//Borrowed from @apead's project https://github.com/apead/NetCoreSkiaGtkLinux

namespace chip8.gtkskia
{
    public class CustomDrawing : DrawingArea
    {
        bool[,] screen = new bool[64, 32];

        public bool[,] ScreenData
        {
            get => screen;
            set
            {
                screen = value;
                this.QueueDraw();
            }
        }

        protected override bool OnDrawn(Cairo.Context cr)
        {
            const int width = 640;
            const int height = 320;

            using (var bitmap = new SKBitmap(width, height, SKColorType.Rgb888x, SKAlphaType.Premul))
            {
                IntPtr len;
                using (var skSurface = SKSurface.Create(bitmap.Info.Width, bitmap.Info.Height, SKColorType.Rgb888x, SKAlphaType.Premul, bitmap.GetPixels(out len), bitmap.Info.RowBytes))
                {
                    var canvas = skSurface.Canvas;
                    canvas.Clear(SKColors.Black);

                    using (var paint = new SKPaint())
                    {
                        for (int x = 0; x < 64; x++)
                            for (int y = 0; y < 32; y++)
                            {
                                if (screen[x, y])
                                {
                                    paint.Color = SKColors.White;
                                    canvas.DrawRect(x * 10, y * 10, 10, 10, paint);
                                }
                                else
                                {
                                    paint.Color = SKColors.Black;
                                    canvas.DrawRect(x * 10, y * 10, 10, 10, paint);
                                }
                            }
                    }

                    using (Cairo.Surface surface = new Cairo.ImageSurface(
                        bitmap.GetPixels(out len),
                        Cairo.Format.Argb32,
                        bitmap.Width, bitmap.Height,
                        bitmap.Width * 4))
                    {
                        surface.MarkDirty();
                        cr.SetSourceSurface(surface, 0, 0);
                        cr.Paint();
                    }
                }
            }

            return true;
        }
    }
}