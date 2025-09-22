using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Objects
{
    internal class HitPoint
    {
        public bool DidHit { get; set; }
        public float Distance { get; set; }
        public Vector3 Point { get; set; }
        public Vector3 Normal { get; set; }
        public Vector3 Color { get; set; }

    }
}
