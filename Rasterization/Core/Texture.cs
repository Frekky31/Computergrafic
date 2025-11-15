using System.Drawing;
using System.Numerics;

namespace Rasterization.Core
{
    public class Texture
    {
        private readonly Vector3[,] pixelsLinear;
        public int Width { get; }
        public int Height { get; }

        public Texture(int w, int h)
        {
            Width = w;
            Height = h;
            pixelsLinear = new Vector3[w, h];
        }

        public void SetPixelLinear(int x, int y, Vector3 linearColor)
        {
            pixelsLinear[x, y] = linearColor;
        }

        public Vector3 SampleBilinear(Vector2 uv)
        {
            if (Width == 0 || Height == 0) return Vector3.One;

            float u = uv.X % 1f; if (u < 0) u += 1f;
            float v = uv.Y % 1f; if (v < 0) v += 1f;

            v = 1f - v;

            float fx = u * (Width - 1);
            float fy = v * (Height - 1);
            int x0 = (int)MathF.Floor(fx);
            int y0 = (int)MathF.Floor(fy);
            int x1 = (x0 + 1) % Width;
            int y1 = (y0 + 1) % Height;
            float sx = fx - x0;
            float sy = fy - y0;

            var c00 = pixelsLinear[x0, y0];
            var c10 = pixelsLinear[x1, y0];
            var c01 = pixelsLinear[x0, y1];
            var c11 = pixelsLinear[x1, y1];

            var cx0 = Vector3.Lerp(c00, c10, sx);
            var cx1 = Vector3.Lerp(c01, c11, sx);
            return Vector3.Lerp(cx0, cx1, sy);
        }
        public static Texture FromBitmapFile(string path)
        {
            using var bmp = new Bitmap(path);
            return FromBitmap(bmp);
        }


        public static Texture FromBitmap(Bitmap bmp)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            var t = new Texture(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var c = bmp.GetPixel(x, y);
                    float rf = c.R / 255f;
                    float gf = c.G / 255f;
                    float bf = c.B / 255f;
                    rf = MathF.Pow(rf, 2.2f);
                    gf = MathF.Pow(gf, 2.2f);
                    bf = MathF.Pow(bf, 2.2f);
                    t.SetPixelLinear(x, y, new Vector3(rf, gf, bf));
                }
            }
            return t;
        }
    }
}
