using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using DrawingColor = System.Drawing.Color;

namespace RayTracing.Core
{
    public class RenderTarget(int w, int h)
    {
        public readonly Vector3[] ColourBuffer = new Vector3[w * h];
        public readonly object[] locks = new object[w * h];

        public readonly int Width = w;
        public readonly int Height = h;
        public readonly Vector2 Size = new(w, h);

        public void Clear(Vector3 bgCol)
        {
            for (int i = 0; i < ColourBuffer.Length; i++)
            {
                ColourBuffer[i] = bgCol;
            }

            if (locks[0] == null)
            {
                for (int i = 0; i < locks.Length; i++)
                {
                    locks[i] = new object();
                }
            }
        }

        public void SaveToFile(string path)
        {
            using var bmp = new Bitmap(Width, Height);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int i = (Height - 1 - y) * Width + x;
                    Vector3 col = ColourBuffer[i];
                    int r = FloatToSrgbByte(col.X);
                    int g = FloatToSrgbByte(col.Y);
                    int b = FloatToSrgbByte(col.Z);
                    bmp.SetPixel(x, y, DrawingColor.FromArgb(255, r, g, b));
                }
            }
            bmp.Save(path, ImageFormat.Png);
        }

        private static int FloatToSrgbByte(float c)
        {
            c = Math.Clamp(c, 0f, 1f);
            c = MathF.Pow(c, 1.0f / 2.2f);
            c = Math.Clamp(c, 0f, 1f);
            return (int)(c * 255);
        }
    }
}
