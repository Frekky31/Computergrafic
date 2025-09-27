using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Objects
{
    public class Cube : RenderObject
    {
        public Vector3 Center { get; set; }
        public Vector3 Size { get; set; }
        public Vector3 Color { get; set; }
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public Triangle[] Triangles { get; set; }
        public Sphere[] Vertices { get; set; }

        public Cube(Vector3 center, Vector3 size, Vector3 color)
        {
            Center = center;
            Size = size;
            Color = color;
            Rotation = Quaternion.Identity;
            Triangles = ToTriangles(true);
            Vertices = ToSpheres();
        }

        public override void Rotate(Quaternion rotation)
        {
            Rotation = rotation * Rotation;
            Triangles = ToTriangles(true);
            Vertices = ToSpheres();
        }

        public override void Move(Vector3 translation)
        {
            Center += translation;
            Triangles = ToTriangles(true);
            Vertices = ToSpheres();
        }

        private Vector3[] GetPoints()
        {
            var halfScale = Size / 2; 
            var localPoints = new Vector3[]
            {
                new(-halfScale.X, -halfScale.Y, -halfScale.Z),
                new(halfScale.X, -halfScale.Y, -halfScale.Z),
                new(halfScale.X, halfScale.Y, -halfScale.Z),
                new(-halfScale.X, halfScale.Y, -halfScale.Z),
                new(-halfScale.X, -halfScale.Y, halfScale.Z),
                new(halfScale.X, -halfScale.Y, halfScale.Z),
                new(halfScale.X, halfScale.Y, halfScale.Z),
                new(-halfScale.X, halfScale.Y, halfScale.Z)
            };
            for (int i = 0; i < localPoints.Length; i++)
            {
                localPoints[i] = Vector3.Transform(localPoints[i], Rotation) + Center;
            }
            return localPoints;
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
            var p = GetPoints();

            var triangles = new Triangle[]
            {
                // Bottom face (Y = -halfScale.Y)
                new(p[0], p[5], p[1], Color),
                new(p[0], p[4], p[5], twoTone ? Color2 : Color),

                // Top face (Y = +halfScale.Y)
                new(p[3], p[6], p[7], Color),
                new(p[3], p[2], p[6], twoTone ? Color2 : Color),

                // Front face (Z = +halfScale.Z)
                new(p[4], p[6], p[5], Color),
                new(p[4], p[7], p[6], twoTone ? Color2 : Color),

                // Back face (Z = -halfScale.Z)
                new(p[0], p[2], p[3], Color),
                new(p[0], p[1], p[2], twoTone ? Color2 : Color),

                // Right face (X = +halfScale.X)
                new(p[1], p[6], p[2], Color),
                new(p[1], p[5], p[6], twoTone ? Color2 : Color),

                // Left face (X = -halfScale.X)
                new(p[0], p[7], p[4], Color),
                new(p[0], p[3], p[7], twoTone ? Color2 : Color),
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

        public override void Scale(float scale)
        {
            Size *= scale;
            Triangles = ToTriangles(true);
            Vertices = ToSpheres();
        }
    }
}
