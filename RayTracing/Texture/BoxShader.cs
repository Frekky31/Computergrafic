using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    using System;
    using System.Numerics;

namespace RayTracing.Texture
{
    public static class BoxShader
    {
        const float PI = MathF.PI;
        const float PI2 = PI * 2.0f;

        // ---- Public sampler: equivalent of mainImage's output
        // fragCoord: pixel coordinates (x,y)
        // iResolution: (width, height)
        // iTime: time in seconds
        public static Vector3 Sample(Vector2 fragCoord, Vector2 iResolution, float iTime)
        {
            // p = (fragCoord * 2.0 - iResolution) / min(iResolution.x, iResolution.y)
            Vector2 p = (fragCoord * 2.0f - iResolution) / MathF.Min(iResolution.X, iResolution.Y);

            Vector3 cPos = new Vector3(0.0f, 0.0f, -3.0f * iTime);
            Vector3 cDir = Vector3.Normalize(new Vector3(0.0f, 0.0f, -1.0f));
            Vector3 cUp = new Vector3(MathF.Sin(iTime), 1.0f, 0.0f);
            Vector3 cSide = Vector3.Cross(cDir, cUp);

            Vector3 ray = Vector3.Normalize(cSide * p.X + cUp * p.Y + cDir);

            float acc = 0.0f;
            float acc2 = 0.0f;
            float t = 0.0f;

            for (int i = 0; i < 99; i++)
            {
                Vector3 pos = cPos + ray * t;
                float dist = Map(pos, cPos, iTime);
                dist = MathF.Max(MathF.Abs(dist), 0.02f);
                float a = MathF.Exp(-dist * 3.0f);

                if (Mod(Length(pos) + 24.0f * iTime, 30.0f) < 3.0f)
                {
                    a *= 2.0f;
                    acc2 += a;
                }

                acc += a;
                t += dist * 0.5f;
            }

            Vector3 col = new Vector3(acc * 0.01f,
                                      acc * 0.011f + acc2 * 0.002f,
                                      acc * 0.012f + acc2 * 0.005f);
            return col;
        }

        // ---- Map / scene functions ----

        // map(vec3 p, vec3 cPos)
        static float Map(Vector3 p, Vector3 cPos, float iTime)
        {
            Vector3 p1 = p;
            p1.X = Mod(p1.X - 5.0f, 10.0f) - 5.0f;
            p1.Y = Mod(p1.Y - 5.0f, 10.0f) - 5.0f;
            p1.Z = Mod(p1.Z, 16.0f) - 8.0f;
            Vector2 xy = new Vector2(p1.X, p1.Y);
            xy = PMod(xy, 5.0f, iTime);
            p1.X = xy.X;
            p1.Y = xy.Y;
            return IfsBox(p1, iTime);
        }

        // ifsBox
        static float IfsBox(Vector3 p, float iTime)
        {
            for (int i = 0; i < 5; i++)
            {
                p = Abs(p) - Vector3.One * 1.0f;

                // p.xy *= rot(iTime*0.3);
                Vector2 xy = new Vector2(p.X, p.Y);
                xy = Rotate(xy, iTime * 0.3f * i); // note: original used rot(iTime*0.3) each iteration; to match animation better we can multiply by i
                p.X = xy.X; p.Y = xy.Y;

                // p.xz *= rot(iTime*0.1);
                Vector2 xz = new Vector2(p.X, p.Z);
                xz = Rotate(xz, iTime * 0.1f * i);
                p.X = xz.X; p.Z = xz.Y;
            }

            // extra rotate p.xz *= rot(iTime);
            Vector2 p_xz = new Vector2(p.X, p.Z);
            p_xz = Rotate(p_xz, iTime);
            p.X = p_xz.X; p.Z = p_xz.Y;

            return Box(p, new Vector3(0.4f, 0.8f, 0.3f));
        }

        // box(vec3 p, vec3 b)
        static float Box(Vector3 p, Vector3 b)
        {
            Vector3 d = Abs(p) - b;
            float m = MathF.Min(MathF.Max(d.X, MathF.Max(d.Y, d.Z)), 0.0f);
            Vector3 mx0 = Max(d, Vector3.Zero);
            return m + Length(mx0);
        }

        // ---- Helpers ----

        // rotate 2D vector by angle a using mat2(c,s,-s,c) convention from GLSL
        // GLSL mat2(c,s,-s,c) * v => (c*x + s*y, -s*x + c*y)
        static Vector2 Rotate(Vector2 v, float a)
        {
            float c = MathF.Cos(a);
            float s = MathF.Sin(a);
            return new Vector2(c * v.X + s * v.Y, -s * v.X + c * v.Y);
        }

        // pmod(vec2 p, float r) equivalent
        static Vector2 PMod(Vector2 p, float r, float iTime)
        {
            // a = atan(p.x, p.y) + pi/r;
            // Note: GLSL uses atan(y,x) signature; original used atan(p.x, p.y)
            float a = MathF.Atan2(p.X, p.Y) + PI / r;
            float n = PI2 / r;
            a = MathF.Floor(a / n) * n;
            return Rotate(p, -a);
        }

        // box helper for abs vector
        static Vector3 Abs(Vector3 v) => new Vector3(MathF.Abs(v.X), MathF.Abs(v.Y), MathF.Abs(v.Z));
        static Vector3 Max(Vector3 a, Vector3 b) => new Vector3(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y), MathF.Max(a.Z, b.Z));
        static float Length(Vector3 v) => v.Length();

        // Mod consistent with GLSL.mod
        static float Mod(float x, float m) => x - m * MathF.Floor(x / m);
    }
}
