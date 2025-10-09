using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Numerics;

namespace RayTracing.Texture
{
    public static class CellularNoise2D
    {
        // Equivalent of GLSL's `random2`
        static Vector2 Random2(Vector2 p)
        {
            float x = Vector2.Dot(p, new Vector2(127.1f, 311.7f));
            float y = Vector2.Dot(p, new Vector2(269.5f, 183.3f));
            Vector2 s = new Vector2(MathF.Sin(x), MathF.Sin(y)) * 43758.5453f;
            return Frac(s);
        }

        // Equivalent of GLSL's `cellular`
        public static float Cellular(Vector2 p)
        {
            Vector2 i_st = Floor(p);
            Vector2 f_st = Frac(p);
            float m_dist = 10.0f;

            for (int j = -1; j <= 1; j++)
                for (int i = -1; i <= 1; i++)
                {
                    Vector2 neighbor = new Vector2(i, j);
                    Vector2 point = Random2(i_st + neighbor);
                    point = 0.5f * new Vector2(
                        MathF.Sin(6.2831f * point.X),
                        MathF.Sin(6.2831f * point.Y)
                    );

                    Vector2 diff = neighbor + point - f_st;
                    float dist = diff.Length();
                    if (dist < m_dist)
                        m_dist = dist;
                }

            return m_dist;
        }

        // === Helpers (GLSL-style) ===
        static Vector2 Floor(Vector2 v) => new Vector2(MathF.Floor(v.X), MathF.Floor(v.Y));
        static Vector2 Frac(Vector2 v) => new Vector2(v.X - MathF.Floor(v.X), v.Y - MathF.Floor(v.Y));
    }

}
