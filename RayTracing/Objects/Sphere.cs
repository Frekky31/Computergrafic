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
        public double radius { get; set; }
        public Vector3 center { get; set; }
        public Vector3 color { get; set; }

        public Sphere() { }
        public Sphere(double radius, Vector3 center, Vector3 color)
        {
            this.radius = radius;
            this.center = center;
            this.color = color;
        }
    }
}
