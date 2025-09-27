using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Objects
{
    public class Sphere(double radius, Vector3 center, Vector3 color) : RenderObject
    {
        public double Radius { get; set; } = radius;
        public Vector3 Center { get; set; } = center;
        public Vector3 Color { get; set; } = color;

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
    }
}
