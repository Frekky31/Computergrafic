using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Numerics;

namespace RayTracing.Texture
{
    public static class StarfieldShader
    {
        const int NUM_LAYERS = 10;

        // Public entry — returns RGBA (linear)
        // fragCoord: MathF.PIxel coordinate (x+0.5, y+0.5) for center sampling
        // iResolution: (width, height)
        // iMouse: mouse position in MathF.PIxels (use Vector2.Zero if unused)
        // iTime: seconds
        // iChannel0_rg: the rg components of texture(iChannel0, vec2(0.25,0.))
        public static Vector3 Sample(Vector2 fragCoord, Vector2 iResolution, Vector2 iChannel0_rg)
        {
            // uv = (fragCoord - 0.5*iResolution)/iResolution.y
            Vector2 uv = (fragCoord - 0.5f * iResolution) / iResolution.Y;

            // initial transforms
            uv.X = MathF.Abs(uv.X);
            uv.Y += MathF.Tan((5.0f / 6.0f) * MathF.PI) * 0.5f;

            Vector2 n = N((5.0f / 6.0f) * MathF.PI);
            float d = Dot(uv - new Vector2(0.5f, 0.0f), n);
            uv -= n * MathF.Max(0.0f, d) * 2.0f;

            n = N((2.0f / 3.0f) * MathF.PI);
            float scale = 1.0f;
            uv.X += 1.5f / 1.25f;

            // fractal-ish tiling loop (port of GLSL for-loop)
            for (int i = 0; i < 5; i++)
            {
                scale *= 1.25f;
                uv *= 1.25f;
                uv.X -= 1.5f;

                uv.X = MathF.Abs(uv.X);
                uv.X -= 0.5f;
                float dotUV = Dot(uv, n);
                uv -= n * MathF.Min(0.0f, dotUV) * 2.0f;
            }

            Vector3 col = Vector3.Zero;

            // layers accumulate
            for (int layer = 0; layer < NUM_LAYERS; layer++)
            {
                float i = layer / (float)NUM_LAYERS;
                float depth = Fract(i);
                float layerScale = Lerp(20.0f, 0.5f, depth);
                float fade = depth * Smoothstep(1.0f, 0.9f, depth);
                col += StarLayer(uv * (layerScale + i * 453.2f), iChannel0_rg) * fade;
            }

            return col;
        }

        // ----------------- Helper functions -----------------

        static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;

        static Vector2 N(float angle) => new Vector2(MathF.Sin(angle), MathF.Cos(angle));

        static float Fract(float x) => x - MathF.Floor(x);
        static Vector2 Fract(Vector2 v) => new Vector2(Fract(v.X), Fract(v.Y));

        static float Lerp(float a, float b, float t) => a + (b - a) * t;

        static float Smoothstep(float edge0, float edge1, float x)
        {
            // handle edge1 == edge0 defensively
            float denom = edge1 - edge0;
            if (MathF.Abs(denom) < 1e-6f) return 0f;
            float t = Math.Clamp((x - edge0) / denom, 0f, 1f);
            return t * t * (3f - 2f * t);
        }

        // ----------------- Star + hash + layer -----------------

        static float Star(Vector2 uv, float flare)
        {
            float col = 0.0f;
            float d = uv.Length();
            // avoid division by zero
            float m = 0.0f;
            if (d > 0.000001f)
                m = 0.02f / d;

            float rays = MathF.Max(0.0f, 1.0f - MathF.Abs(uv.X * uv.Y * 1000.0f));
            m += rays * flare;

            uv = MultiplyByRot(uv, MathF.PI / 4.0f);
            rays = MathF.Max(0.0f, 1.0f - MathF.Abs(uv.X * uv.Y * 1000.0f));
            m += rays * 0.3f * flare;

            m *= Smoothstep(1.0f, 0.2f, d);

            return m;
        }

        static Vector2 MultiplyByRot(Vector2 v, float a)
        {
            // same as Rotate but keeps a clearer name for local use
            float c = MathF.Cos(a), s = MathF.Sin(a);
            return new Vector2(c * v.X - s * v.Y, s * v.X + c * v.Y);
        }

        static float Hash21(Vector2 p)
        {
            p = Fract(p * new Vector2(123.34f, 456.21f));
            float dp = p.X * (p.X) + p.Y * (p.Y + 45.32f); // intermediate combining
                                                           // original does p += dot(p, p+45.32) then fract(p.x*p.y)
                                                           // We'll compute dot(p, p+45.32) explicitly:
            float dotVal = p.X * (p.X + 45.32f) + p.Y * (p.Y + 45.32f);
            // add it to p components to better match original influence
            p += new Vector2(dotVal, dotVal);
            return Fract(p.X * p.Y);
        }

        static Vector3 StarLayer(Vector2 uv, Vector2 iChannel0_rg)
        {
            Vector3 col = Vector3.Zero;

            Vector2 gv = Fract(uv) - new Vector2(0.5f, 0.5f);
            Vector2 id = new Vector2((float)MathF.Floor(uv.X), (float)MathF.Floor(uv.Y));

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    Vector2 offs = new Vector2(x, y);
                    float n = Hash21(id + offs);
                    float size = Fract(n * 345.32f);

                    Vector2 p = new Vector2(n, Fract(n * 34.0f));

                    float star = Star(gv - offs - p + new Vector2(0.5f, 0.5f), Smoothstep(0.8f, 1.0f, size) * 0.6f);

                    // hueShift = fract(n*2345.2 + dot(uv /420.,texture(iChannel0, vec2(0.25, 0.)).rg))*vec3(.2, .3, .9)*123.2;
                    // We model texture(iChannel0,vec2(0.25,0.)).rg by the passed iChannel0_rg vector.
                    float channelDot = Dot(uv / 420.0f, iChannel0_rg);
                    Vector3 hueShift = new Vector3(Fract(n * 2345.2f + channelDot),
                                                   Fract(n * 2345.2f + channelDot),
                                                   Fract(n * 2345.2f + channelDot));
                    hueShift *= new Vector3(0.2f, 0.3f, 0.9f) * 123.2f;

                    Vector3 color = new Vector3(MathF.Sin(hueShift.X), MathF.Sin(hueShift.Y), MathF.Sin(hueShift.Z)) * 0.5f + new Vector3(0.5f);
                    color = color * new Vector3(1.0f, 0.25f, 1.0f + size);

                    star *= (MathF.Sin(3.0f + n * 6.2831f) * 0.4f + 1.0f);
                    col += star * size * color;
                }
            }

            return col;
        }
    }

}
