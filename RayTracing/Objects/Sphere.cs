using RayTracing.Texture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Objects
{
    public class Sphere : RenderObject
    {
        public float Radius { get; set; }
        public Vector3 Center { get; set; }

        public Sphere(float radius, Vector3 center, Material material)
        {
            Radius = radius;
            Center = center;
            Material = material;
        }

        public override Span<Triangle> GetTriangles()
        {
            return [];
        }

        public override Span<Sphere> GetSpheres()
        {
            Sphere[] spheres = [this];
            return new Span<Sphere>(spheres);
        }

        public override void Move(Vector3 translation)
        {
            Center += translation;
        }

        public override void Rotate(Quaternion rotation)
        {

        }

        public override void Scale(float scale)
        {
            Radius *= scale;
        }


        public static Vector2 GetSphereUV(Vector3 point, Sphere sphere)
        {
            Vector3 p = Vector3.Normalize(point - sphere.Center);
            float u = 0.5f + (float)(MathF.Atan2(p.Z, p.X) / (2 * MathF.PI));
            float v = 0.5f - (float)(MathF.Asin(p.Y) * 1f / MathF.PI);
            return new Vector2(u, v);
        }

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
