using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Objects
{
    public class Cube
    {
        public Vector3 Center { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Color { get; set; }
        public Cube() { }
        public Cube(Vector3 center, Vector3 scale, Vector3 color)
        {
            Center = center;
            Scale = scale;
            Color = color;
        }

        public List<Sphere> ToSpheres()
        {
            var Color2 = new Vector3(1, 1, 1);
            var radius = 0.01;
            var halfScale = Scale / 2;
            var p0 = Center + new Vector3(-halfScale.X, -halfScale.Y, -halfScale.Z);
            var p1 = Center + new Vector3(halfScale.X, -halfScale.Y, -halfScale.Z);
            var p2 = Center + new Vector3(halfScale.X, halfScale.Y, -halfScale.Z);
            var p3 = Center + new Vector3(-halfScale.X, halfScale.Y, -halfScale.Z);
            var p4 = Center + new Vector3(-halfScale.X, -halfScale.Y, halfScale.Z);
            var p5 = Center + new Vector3(halfScale.X, -halfScale.Y, halfScale.Z);
            var p6 = Center + new Vector3(halfScale.X, halfScale.Y, halfScale.Z);
            var p7 = Center + new Vector3(-halfScale.X, halfScale.Y, halfScale.Z);

            var spheres = new List<Sphere>
            {
                new(radius,p0,Color2),
                new(radius,p1,Color2),
                new(radius,p2,Color2),
                new(radius,p3,Color2),
                new(radius,p4,Color2),
                new(radius,p5,Color2),
                new(radius,p6,Color2),
                new(radius,p7,Color2)
            };

            return spheres;
        }

        public List<Triangle> ToTriangles(bool twoTone = false)
        {
            var Color2 = Color * 0.5f;
            var halfScale = Scale / 2;
            var p0 = Center + new Vector3(-halfScale.X, -halfScale.Y, -halfScale.Z);
            var p1 = Center + new Vector3(halfScale.X, -halfScale.Y, -halfScale.Z);
            var p2 = Center + new Vector3(halfScale.X, halfScale.Y, -halfScale.Z);
            var p3 = Center + new Vector3(-halfScale.X, halfScale.Y, -halfScale.Z);
            var p4 = Center + new Vector3(-halfScale.X, -halfScale.Y, halfScale.Z);
            var p5 = Center + new Vector3(halfScale.X, -halfScale.Y, halfScale.Z);
            var p6 = Center + new Vector3(halfScale.X, halfScale.Y, halfScale.Z);
            var p7 = Center + new Vector3(-halfScale.X, halfScale.Y, halfScale.Z);

            var triangles = new List<Triangle>
            {
                // Bottom face (-Z)
                new(p0, p1, p2, Color),
                new(p0, p2, p3, twoTone ? Color2 : Color),

                // Top face (+Z)
                new(p4, p6, p5, Color),
                new(p4, p7, p6, twoTone ? Color2 : Color),

                // Front face (+Y)
                new(p3, p2, p6, Color),
                new(p3, p6, p7, twoTone ? Color2 : Color),

                // Back face (-Y)
                new(p0, p5, p1, Color),
                new(p0, p4, p5, twoTone ? Color2 : Color),

                // Right face (+X)
                new(p1, p5, p6, Color),
                new(p1, p6, p2, twoTone ? Color2 : Color),

                // Left face (-X)
                new(p0, p3, p7, Color),
                new(p0, p7, p4, twoTone ? Color2 : Color),
            };

            return triangles;
        }
    }
}
