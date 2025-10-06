using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Core
{
    public class HitPoint
    {
        public bool DidHit { get; set; }
        public float Distance { get; set; }
        public Vector3 Point { get; set; }
        public Vector3 Normal { get; set; }
        public Material Material { get; set; } = new();

    }
}
