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
        public Material Material { get; set; } = new();
        public Cube() { }
        public Cube(Vector3 center, Vector3 scale, Material material)
        {
            Center = center;
            Scale = scale;
            Material = material;
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
                new(radius,p0,new(Color2, Vector3.Zero, Vector3.Zero)),
                new(radius,p1,new(Color2, Vector3.Zero, Vector3.Zero)),
                new(radius,p2,new(Color2, Vector3.Zero, Vector3.Zero)),
                new(radius,p3,new(Color2, Vector3.Zero, Vector3.Zero)),
                new(radius,p4,new(Color2, Vector3.Zero, Vector3.Zero)),
                new(radius,p5,new(Color2, Vector3.Zero, Vector3.Zero)),
                new(radius,p6,new(Color2, Vector3.Zero, Vector3.Zero)),
                new(radius,p7,new(Color2, Vector3.Zero, Vector3.Zero))
            };

            return spheres;
        }

        public List<Triangle> ToTriangles(bool twoTone = false)
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

            var triangles = new List<Triangle>
            {
                // Bottom face (-Z)
                new(p0, p1, p2, Material),
                new(p0, p2, p3, Material),

                // Top face (+Z)
                new(p4, p6, p5, Material    ),
                new(p4, p7, p6, Material),

                // Front face (+Y)
                new(p3, p2, p6, Material),
                new(p3, p6, p7, Material),

                // Back face (-Y)
                new(p0, p5, p1, Material),
                new(p0, p4, p5, Material),

                // Right face (+X)
                new(p1, p5, p6, Material),
                new(p1, p6, p2, Material),

                // Left face (-X)
                new(p0, p3, p7, Material),
                new(p0, p7, p4, Material),
            };

            return triangles;
        }
    }
}
