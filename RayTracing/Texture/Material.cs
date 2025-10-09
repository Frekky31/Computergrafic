using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RayTracing.Texture
{
    public class Material
    {
        public Vector3 Diffuse { get; set; } = Vector3.One;
        public Vector3 Emission { get; set; }
        public Vector3 Specular { get; set; }

        public Bitmap? Texture { get; set; } = null;

        public Func<Vector2, Vector3>? ProceduralTexture { get; set; }

        public float SpecularDistance { get; set; } = 0.01f;

        public float[]? HDRTexture { get; set; } // RGB float array
        public int HDRWidth { get; set; }
        public int HDRHeight { get; set; }

        public bool HasTexture => Texture != null || ProceduralTexture != null || HDRTexture != null;

        public static Vector3 SampleTexture(Material material, Vector2 uv)
        {
            if (material.HDRTexture != null)
                return SampleHDRTexture(material, uv);

            if (material.ProceduralTexture != null)
                return material.ProceduralTexture(uv);

            if (material.Texture != null)
            {
                lock (material.Texture)
                {
                    int x = Math.Clamp((int)(uv.X * material.Texture.Width), 0, material.Texture.Width - 1);
                    int y = Math.Clamp((int)(uv.Y * material.Texture.Height), 0, material.Texture.Height - 1);
                    var color = material.Texture.GetPixel(x, y);
                    return new Vector3(
                        (float)MathF.Pow(color.R / 255f, 2.2f),
                        (float)MathF.Pow(color.G / 255f, 2.2f),
                        (float)MathF.Pow(color.B / 255f, 2.2f)
                    );
                }
            }

            return material.Diffuse;
        }

        public static Vector3 SampleHDRTexture(Material material, Vector2 uv)
        {
            if (material.HDRTexture == null) return material.Diffuse;

            int x = Math.Clamp((int)(uv.X * material.HDRWidth), 0, material.HDRWidth - 1);
            int y = Math.Clamp((int)(uv.Y * material.HDRHeight), 0, material.HDRHeight - 1);
            int idx = (y * material.HDRWidth + x) * 3;
            return new Vector3(
                material.HDRTexture[idx],
                material.HDRTexture[idx + 1],
                material.HDRTexture[idx + 2]
            );
        }
    }
}
