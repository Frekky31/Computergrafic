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
        public Triangle[] Triangles { get; set; }
        public Sphere[] Vertices { get; set; }

        public Cube(Vector3 center, Vector3 scale, Vector3 color)
        {
            Center = center;
            Scale = scale;
            Color = color;
            Triangles = ToTriangles(true);
            Vertices = ToSpheres();
        }

        private Vector3[] GetPoints()
        {
            var halfScale = Scale / 2; 
            var p0 = Center + new Vector3(-halfScale.X, -halfScale.Y, -halfScale.Z);
            var p1 = Center + new Vector3(halfScale.X, -halfScale.Y, -halfScale.Z);
            var p2 = Center + new Vector3(halfScale.X, halfScale.Y, -halfScale.Z);
            var p3 = Center + new Vector3(-halfScale.X, halfScale.Y, -halfScale.Z);
            var p4 = Center + new Vector3(-halfScale.X, -halfScale.Y, halfScale.Z);
            var p5 = Center + new Vector3(halfScale.X, -halfScale.Y, halfScale.Z);
            var p6 = Center + new Vector3(halfScale.X, halfScale.Y, halfScale.Z);
            var p7 = Center + new Vector3(-halfScale.X, halfScale.Y, halfScale.Z);
            return [p0, p1, p2, p3, p4, p5, p6, p7];
        }

        private Sphere[] ToSpheres()
        {
            var Color2 = Color * new Vector3(0.05f, 0.05f, 0.05f);
            var radius = 0.01;

            var points = GetPoints();

            var spheres = new Sphere[]
            {
                new(radius,points[0],Color2),
                new(radius,points[1],Color2),
                new(radius,points[2],Color2),
                new(radius,points[3],Color2),
                new(radius,points[4],Color2),
                new(radius,points[5],Color2),
                new(radius,points[6],Color2),
                new(radius,points[7],Color2)
            };

            return spheres;
        }

        private Triangle[] ToTriangles(bool twoTone = false)
        {
            var Color2 = Color * 0.5f;
            var points = GetPoints();

            var triangles = new Triangle[]
            {
                // Bottom face (-Z)
                new(points[0], points[1], points[2], Color),
                new(points[0], points[2], points[3], twoTone ? Color2 : Color),

                // Top face (+Z)
                new(points[4], points[6], points[5], Color),
                new(points[4], points[7], points[6], twoTone ? Color2 : Color),

                // Front face (+Y)
                new(points[3], points[2], points[6], Color),
                new(points[3], points[6], points[7], twoTone ? Color2 : Color),

                // Back face (-Y)
                new(points[0], points[5], points[1], Color),
                new(points[0], points[4], points[5], twoTone ? Color2 : Color),

                // Right face (+X)
                new(points[1], points[5], points[6], Color),
                new(points[1], points[6], points[2], twoTone ? Color2 : Color),

                // Left face (-X)
                new(points[0], points[3], points[7], Color),
                new(points[0], points[7], points[4], twoTone ? Color2 : Color),
            };

            return triangles;
        }
    }
}
