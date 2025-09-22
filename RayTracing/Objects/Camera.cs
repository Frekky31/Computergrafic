using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Objects
{
    internal class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 LookAt { get; set; }
        public Vector3 Up { get; set; }
        public float Fov { get; set; }
    }
}
