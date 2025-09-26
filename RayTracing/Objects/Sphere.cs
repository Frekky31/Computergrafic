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
        public Vector3 Color { get; set; }

        public Sphere(double radius, Vector3 center, Vector3 color)
        {
            Radius = radius;
            Center = center;
            Color = color;
        }
    }
}
