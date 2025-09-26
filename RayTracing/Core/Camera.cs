using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Core
{
    public class Camera(Vector3 position, Vector3 lookAt, Vector3 up, float fov)
    {
        public Vector3 Position { get; set; } = position;
        public Vector3 LookAt { get; set; } = lookAt;
        public Vector3 Up { get; set; } = up;
        public float Fov { get; set; } = fov;
    }
}
