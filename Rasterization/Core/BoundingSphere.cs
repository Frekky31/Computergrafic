using System.Numerics;

namespace Rasterization.Core
{
    public record BoundingSphere(Vector3 Center, float Radius)
    {
        public BoundingSphere Transform(Matrix4x4 m)
        {
            Vector3 newCenter = Vector3.Transform(Center, m);

            float sx = new Vector3(m.M11, m.M12, m.M13).Length();
            float sy = new Vector3(m.M21, m.M22, m.M23).Length();
            float sz = new Vector3(m.M31, m.M32, m.M33).Length();
            float scale = MathF.Max(sx, MathF.Max(sy, sz));

            return new BoundingSphere(newCenter, Radius * scale);
        }
    }
}
