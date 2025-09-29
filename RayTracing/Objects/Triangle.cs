using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Objects
{
    public class Triangle
    {
        public Vector3 A { get; set; }
        public Vector3 B { get; set; }
        public Vector3 C { get; set; }

        public Material Material { get; set; } = new();

        public Triangle() { }

        public Triangle(Vector3 a, Vector3 b, Vector3 c, Material material)
        {
            A = a;
            B = b;
            C = c;
            Material = material;
        }
    }
}
