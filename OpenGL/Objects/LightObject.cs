using System.Numerics;

namespace OpenGL.Objects
{
    public class LightObject
    {
        public Vector3 Position { get; set; } = Vector3.Zero;    // world-space position (for point/spot)
        public Vector3 Color { get; set; } = new Vector3(1, 1, 1);
        public float Intensity { get; set; } = 1.0f;


        public LightObject() { }

        public LightObject(Vector3 color, Vector3 pos, float intensity = 1.0f)
        {
            Color = color;
            Intensity = intensity;
            Position = pos;
        }
    }
}
