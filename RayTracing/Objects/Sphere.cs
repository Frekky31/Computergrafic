using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Objects
{
    public class Sphere
    {
        public double Radius { get; set; }
        public Vector3 Center { get; set; }
        public Material Material { get; set; } = new();

        public Sphere() { }

        public Sphere(double radius, Vector3 center, Material material)
        {
            Radius = radius;
            Center = center;
            Material = material;
        }
    }
}
