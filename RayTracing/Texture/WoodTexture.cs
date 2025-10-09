using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Texture
{
    public class WoodTexture
    {
        public static Vector3 Sample(Vector2 uv, float aspect = 1.0f /* width/height */)
        {
            // st = gl_FragCoord.xy / u_resolution.xy, but here we take uv in [0,1]
            Vector2 st = uv;
            st.X *= aspect;

            // v0 = wood rings with fbm wobble
            // fbm(st.xx * vec2(100, 12)) -> fbm(Vector2(st.x*100, st.x*12))
            float fbmTerm = FBM(new Vector2(st.X * 100f, st.X * 12f));
            float v0 = Smoothstep(-1.0f, 1.0f, MathF.Sin(st.X * 14.0f + fbmTerm * 8.0f));

            // v1 = small random color variation per tile
            float v1 = Random(st);

            // v2 = fine noise detail
            float nA = Noise(new Vector2(st.X * 200.0f, st.Y * 14.0f));
            float nB = Noise(new Vector2(st.X * 1000.0f, st.Y * 64.0f));
            float v2 = nA - nB;

            // base/palette mixes
            Vector3 col = new Vector3(0.860f, 0.806f, 0.574f);
            col = Lerp(col, new Vector3(0.390f, 0.265f, 0.192f), v0);
            col = Lerp(col, new Vector3(0.930f, 0.493f, 0.502f), v1 * 0.5f);
            col -= new Vector3(v2 * 0.2f, v2 * 0.2f, v2 * 0.2f);

            // clamp for safety
            return Vector3.Clamp(col, Vector3.Zero, Vector3.One);
        }

        // ===== Noise stack =====

        // GLSL random(vec2)
        static float Random(Vector2 st)
        {
            float d = Vector2.Dot(st, new Vector2(12.9898f, 78.233f));
            return Frac(MathF.Sin(d) * 43758.5453123f);
        }

        // Morgan McGuire-style value noise
        static float Noise(Vector2 st)
        {
            Vector2 i = Floor(st);
            Vector2 f = Frac(st);

            float a = Random(i);
            float b = Random(i + new Vector2(1.0f, 0.0f));
            float c = Random(i + new Vector2(0.0f, 1.0f));
            float d = Random(i + new Vector2(1.0f, 1.0f));

            Vector2 u = f * f * (new Vector2(3.0f) - 2.0f * f);

            // mix/mad expanded
            return Lerp(a, b, u.X) +
                   (c - a) * u.Y * (1.0f - u.X) +
                   (d - b) * u.X * u.Y;
        }

        const int OCTAVES = 6;
        static float FBM(Vector2 st)
        {
            float value = 0.0f;
            float amplitude = 0.5f;

            for (int i = 0; i < OCTAVES; i++)
            {
                value += amplitude * Noise(st);
                st *= 2.0f;
                amplitude *= 0.5f;
            }
            return value;
        }

        // ===== Helpers (GLSL-like) =====

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
