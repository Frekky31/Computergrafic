using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.Objects
{
    public class LightObject
    {
        public Vector3 Color { get; set; } = new Vector3(1, 1, 1);
        public Vector3 Position { get; set; } = Vector3.Zero;
        public LightObject() { }

    }
}
