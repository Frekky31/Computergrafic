using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Objects
{
    public class Rectangle
    {
        public Vector3 Corner { get; set; }
        public Vector3 SideVector1 { get; set; }
        public Vector3 SideVector2 { get; set; }
        public Vector3 Color { get; set; }
        public Rectangle() { }

        public Rectangle(Vector3 corner, Vector3 sideVector1, Vector3 sideVector2, Vector3 color)
        {
            Corner = corner;
            SideVector1 = sideVector1;
            SideVector2 = sideVector2;
            Color = color;
        }

        public List<Sphere> ToSpheres()
        {
            var Color2 = new Vector3(1, 1, 1);
            var radius = 0.01;

            return [.. FromCornerAndSides(Corner, SideVector1, SideVector2).Select(x => new Sphere(radius, x, Color2))];
        }

        public static Vector3[] FromCornerAndSides(Vector3 corner, Vector3 u, Vector3 v, bool orthogonalize = true)
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

        public List<Triangle> ToTriangles(bool twoTone = false)
        {
            var Color2 = Color * 0.5f;

            var corners = FromCornerAndSides(Corner, SideVector1, SideVector2);
            var triangles = new List<Triangle>
            {
                new(corners[0], corners[1], corners[2], Color),
                new(corners[1], corners[2], corners[3], twoTone ? Color2 : Color),
            };

            return triangles;
        }
    }
}
