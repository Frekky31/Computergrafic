using RayTracing.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Objects
{
    public class Rectangle : RenderObject
    {
        public Vector3 Corner { get; set; }
        public Vector3 SideVector1 { get; set; }
        public Vector3 SideVector2 { get; set; }

        public Triangle[] Triangles { get; set; }
        public Sphere[] Vertices { get; set; }

        public Rectangle(Vector3 corner, Vector3 sideVector1, Vector3 sideVector2, Material material)
        {
            Corner = corner;
            SideVector1 = sideVector1;
            SideVector2 = sideVector2;
            Material = material;
            Triangles = ToTriangles();
            Vertices = ToSpheres();
        }

        private Sphere[] ToSpheres()
        {
            var radius = 0.01f;

            return [.. FromCornerAndSides(Corner, SideVector1, SideVector2).Select(x => new Sphere(radius, x, Material))];
        }

        private static Vector3[] FromCornerAndSides(Vector3 corner, Vector3 u, Vector3 v, bool orthogonalize = true)
        {
            if (orthogonalize)
            {
                Vector3 uN = Vector3.Normalize(u);
                v -= Vector3.Dot(v, uN) * uN;
            }

            Vector3 p0 = corner;
            Vector3 p1 = corner + u;
            Vector3 p2 = corner + v;
            Vector3 p3 = corner + u + v;

            return [p0, p1, p2, p3];
        }

        private Triangle[] ToTriangles()
        {
            var corners = FromCornerAndSides(Corner, SideVector1, SideVector2);
            var triangles = new Triangle[]
            {
                new(corners[0], corners[1], corners[2], Material),
                new(corners[1], corners[3], corners[2], Material), // fixed winding
            };

            return triangles;
        }

        public override Span<Triangle> GetTriangles()
        {
            return Triangles;
        }

        public override Span<Sphere> GetSpheres()
        {
            return Vertices;
        }

        public override void Move(Vector3 translation)
        {
            Corner += translation;
            Triangles = ToTriangles();
            Vertices = ToSpheres();
        }

        public override void Rotate(Quaternion rotation)
        {
            SideVector1 = Vector3.Transform(SideVector1, rotation);
            SideVector2 = Vector3.Transform(SideVector2, rotation);
            Triangles = ToTriangles();
            Vertices = ToSpheres();
        }

        public override void Scale(float scale)
        {
            SideVector1 *= scale;
            SideVector2 *= scale;
            Triangles = ToTriangles();
            Vertices = ToSpheres();
        }
    }
}
