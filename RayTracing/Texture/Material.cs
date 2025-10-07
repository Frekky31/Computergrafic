using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RayTracing.Texture
{
    public class Material
    {
        public Vector3 Diffuse { get; set; }
        public Vector3 Emission { get; set; }
        public Vector3 Specular { get; set; }

        public Bitmap? Texture { get; set; } = null;

        public Func<Vector2, Vector3>? ProceduralTexture { get; set; }

        public float SpecularDistance { get; set; } = 0.01f;
    }
}
