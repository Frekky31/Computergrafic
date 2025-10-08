using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Texture
{
    internal class LavaLampTexture
    {
        // === Public entry ===
        // uv: texture coordinate [0,1]
        // time: optional animation parameter
        public static Vector3 Sample(Vector2 uv, float time = 0f)
        {
            // Repeat every 1.0
            uv = Frac(uv);

            // Scale & animate
            float scale = 4.0f;
            Vector2 p = uv * scale + new Vector2(0.3f * time, 0.15f * time);

            // Domain warp using fBm
            Vector2 warp = new Vector2(
                FBM(p + new Vector2(3.7f, 2.2f), 4),
                FBM(p + new Vector2(7.1f, 5.3f), 4)
            );
            p += 0.8f * (warp - new Vector2(0.5f));

            // Worley-style cell distance
            float d = Worley(p);

            // Layered glow
            float veins = Smoothstep(0.15f, 0.50f, 1f - d);
            float core = Smoothstep(0.35f, 0.60f, 1f - d);
            float sparkle = Smoothstep(0.05f, 0.10f, 1f - d);

            // Colors
            Vector3 cool = new Vector3(0.25f, 0.05f, 0.60f);
            Vector3 mid = new Vector3(0.95f, 0.20f, 0.35f);
            Vector3 hot = new Vector3(1.00f, 0.75f, 0.25f);

            Vector3 col = Lerp(cool, mid, veins);
            col = Lerp(col, hot, core);

            // Add emissive feel
            float glow = 0.6f * core + 0.4f * veins + 0.2f * sparkle;
            col *= (0.6f + 0.8f * glow);

            // Optional subtle liquid bands
            float bands = 0.5f + 0.5f * MathF.Sin(6.28318f * (p.X + p.Y * 0.5f));
            col *= 0.9f + 0.1f * bands;

            return Vector3.Clamp(col, Vector3.Zero, Vector3.One);
        }

        // === Noise & helpers ===
        static float Worley(Vector2 p)
        {
            Vector2 pi = Floor(p);
            float minDist = 1e9f;

            for (int j = -1; j <= 1; j++)
                for (int i = -1; i <= 1; i++)
                {
                    Vector2 cell = pi + new Vector2(i, j);
                    Vector2 rnd = Hash2(cell);
                    Vector2 feature = cell + rnd;
                    float d = Vector2.Distance(p, feature);
                    if (d < minDist) minDist = d;
                }
            return MathF.Min(minDist, 1.0f);
        }

        static float FBM(Vector2 p, int octaves)
        {
            float f = 0f, amp = 0.5f;
            for (int i = 0; i < octaves; i++)
            {
                f += amp * ValueNoise(p);
                p *= 2.03f;
                amp *= 0.5f;
            }
            return f;
        }

        static float ValueNoise(Vector2 p)
        {
            Vector2 pi = Floor(p);
            Vector2 pf = Frac(p);

            float c00 = Hash1(pi + new Vector2(0, 0));
            float c10 = Hash1(pi + new Vector2(1, 0));
            float c01 = Hash1(pi + new Vector2(0, 1));
            float c11 = Hash1(pi + new Vector2(1, 1));

            Vector2 w = pf * pf * (new Vector2(3f) - 2f * pf);

            float x0 = Lerp(c00, c10, w.X);
            float x1 = Lerp(c01, c11, w.X);
            return Lerp(x0, x1, w.Y);
        }

        static float Hash1(Vector2 p)
        {
            float h = Vector2.Dot(p, new Vector2(127.1f, 311.7f));
            return Frac(MathF.Sin(h) * 43758.5453f);
        }

        static Vector2 Hash2(Vector2 p)
        {
            return new Vector2(
                Hash1(p + new Vector2(1.0f, 0.0f)),
                Hash1(p + new Vector2(0.0f, 1.0f))
            );
        }

        static Vector2 Floor(Vector2 v) => new Vector2(MathF.Floor(v.X), MathF.Floor(v.Y));
        static Vector2 Frac(Vector2 v) => new Vector2(Frac(v.X), Frac(v.Y));
        static float Frac(float x) => x - MathF.Floor(x);
        static float Lerp(float a, float b, float t) => a + (b - a) * t;
        static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a + (b - a) * t;
        static float Smoothstep(float a, float b, float x)
        {
            float t = Math.Clamp((x - a) / (b - a), 0f, 1f);
            return t * t * (3f - 2f * t);
        }
    }

}
