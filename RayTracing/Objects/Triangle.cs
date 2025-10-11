using RayTracing.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Objects
{
    public class Triangle : RenderObject
    {
        public Vector3 A { get; set; }
        public Vector3 B { get; set; }
        public Vector3 C { get; set; }

        public Vector3 EdgeAB { get; set; }
        public Vector3 EdgeAC { get; set; }
        public Vector3 NormalUnit;

        public Triangle(Vector3 a, Vector3 b, Vector3 c, Material material)
        {
            A = a;
            B = b;
            C = c;
            Material = material;
            EdgeAB = B - A;
            EdgeAC = C - A;
            NormalUnit = -Vector3.Normalize(Vector3.Cross(EdgeAB, EdgeAC));
            Material = material;
        }


        public override Span<Triangle> GetTriangles()
        {
            Triangle[] triangles = [this];
            return new Span<Triangle>(triangles);
        }

        public override Span<Sphere> GetSpheres()
        {
            return [];
        }

        public override void Move(Vector3 translation)
        {
            A += translation;
            B += translation;
            C += translation;
            EdgeAB = B - A;
            EdgeAC = C - A;
            NormalUnit = -Vector3.Normalize(Vector3.Cross(EdgeAB, EdgeAC));
        }

        public override void Rotate(Quaternion rotation)
        {
            A = Vector3.Transform(A, rotation);
            B = Vector3.Transform(B, rotation);
            C = Vector3.Transform(C, rotation);
            EdgeAB = B - A;
            EdgeAC = C - A;
            NormalUnit = -Vector3.Normalize(Vector3.Cross(EdgeAB, EdgeAC));
        }

        public override void Scale(float scale)
        {
            A *= scale;
            B *= scale;
            C *= scale;
            EdgeAB = B - A;
            EdgeAC = C - A;
            NormalUnit = -Vector3.Normalize(Vector3.Cross(EdgeAB, EdgeAC));
        }
    }
}