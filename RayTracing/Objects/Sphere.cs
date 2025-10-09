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
    }
}
