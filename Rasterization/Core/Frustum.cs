using System.Numerics;

namespace Rasterization.Core
{
    public class Frustum
    {
        private readonly Vector4[] planes = new Vector4[6];

        private Frustum() { }
        public static Frustum FromMatrix(Matrix4x4 m)
        {
            var f = new Frustum();

            Vector4 row1 = new(m.M11, m.M12, m.M13, m.M14);
            Vector4 row2 = new(m.M21, m.M22, m.M23, m.M24);
            Vector4 row3 = new(m.M31, m.M32, m.M33, m.M34);
            Vector4 row4 = new(m.M41, m.M42, m.M43, m.M44);

            f.planes[0] = NormalizePlane(row4 + row1);
            f.planes[1] = NormalizePlane(row4 - row1);
            f.planes[2] = NormalizePlane(row4 + row2);
            f.planes[3] = NormalizePlane(row4 - row2);
            f.planes[4] = NormalizePlane(row4 + row3);
            f.planes[5] = NormalizePlane(row4 - row3);

            return f;
        }

        private static Vector4 NormalizePlane(Vector4 p)
        {
            Vector3 n = new(p.X, p.Y, p.Z);
            float len = n.Length();
            return len > 1e-6f ? p / len : p;
        }

        public bool ContainsSphere(Vector3 center, float radius)
        {
            for (int i = 0; i < 6; i++)
            {
                var p = planes[i];
                float distance = Vector3.Dot(new Vector3(p.X, p.Y, p.Z), center) + p.W;

                if (distance < -radius)
                    return false;
            }
            return true;
        }
    }
}
