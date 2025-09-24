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

        public List<Triangle> ToTriangles()
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

            // Left-handed, Z-up: +X right, +Y forward, +Z up
            // Each face: two triangles, CCW winding for outward normals

            var triangles = new List<Triangle>
            {
                // Bottom face (-Z)
                new Triangle(p0, p1, p2, Color),
                new Triangle(p0, p2, p3, Color2),

                // Top face (+Z)
                new Triangle(p4, p6, p5, Color),
                new Triangle(p4, p7, p6, Color2),

                // Front face (+Y)
                new Triangle(p3, p2, p6, Color),
                new Triangle(p3, p6, p7, Color2),

                // Back face (-Y)
                new Triangle(p0, p5, p1, Color),
                new Triangle(p0, p4, p5, Color2),

                // Right face (+X)
                new Triangle(p1, p5, p6, Color),
                new Triangle(p1, p6, p2, Color2),

                // Left face (-X)
                new Triangle(p0, p3, p7, Color),
                new Triangle(p0, p7, p4, Color2),
            };

            return triangles;
        }
    }
}
