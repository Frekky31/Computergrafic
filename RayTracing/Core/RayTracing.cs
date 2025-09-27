using RayTracing.Objects;
using RayTracing.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace RayTracing.Core
{
    public class RayTracing
    {
        public static void Render(RenderTarget target, Scene scene)
        {
            int width = target.Width;
            int height = target.Height;
            int pixelCount = width * height;

            var cam = scene.Camera;
            Vector3 f = Vector3.Normalize(cam.LookAt - cam.Position);
            Vector3 r = Vector3.Normalize(Vector3.Cross(cam.Up, f));
            Vector3 u = Vector3.Normalize(Vector3.Cross(r, f));

            float scale = MathF.Tan(cam.Fov * MathF.PI / 360f);
            float aspect = width / (float)height;
            float invW = 1f / width;
            float invH = 1f / height;

            Parallel.For(0, pixelCount, i =>
            {
                int y = i / width;
                int x = i % width;

                float py = 1 - 2 * ((y + 0.5f) * invH);
                float px = (2 * ((x + 0.5f) * invW) - 1) * aspect;

                Vector3 d = f + (py * scale) * u + (px * scale) * r;
                d = Vector3.Normalize(d);
                Vector3 o = cam.Position;

                if (FindClosestHitPoint(scene, o, d, out HitPoint? hit))
                {
                    target.ColourBuffer[i] = hit?.Color ?? Vector3.Zero;
                }
                else
                {
                    target.ColourBuffer[i] = Vector3.Zero;
                }
            });
        }

        private static bool FindClosestHitPoint(in Scene s, in Vector3 o, in Vector3 d, out HitPoint? hit)
        {
            float closest = float.PositiveInfinity;
            HitPoint? best = default;
            bool found = false;

            foreach (var sphere in s.Spheres)
            {
                if (SphereRay(o, d, sphere, out var dist))
                {
                    if (dist < closest)
                    {
                        closest = dist;
                        Vector3 p = o + dist * d;
                        var normal = Vector3.Normalize(p - sphere.Center);
                        best = new HitPoint { DidHit = true, Color = sphere.Color, Distance = dist, Point = p, Normal = normal};
                        found = true;
                    }
                }
            }

            foreach (var triangle in s.Triangles)
            {
                if (Vector3.Dot(d, triangle.NormalUnit) >= 0f)
                    continue;

                if (TriangleRay(o, d, triangle, out var dist))
                {
                    if (dist < closest)
                    {
                        closest = dist;
                        best = new HitPoint { DidHit = true, Color = triangle.Color, Distance = dist, Point = o + dist * d, Normal= triangle.NormalUnit };
                        found = true;
                    }
                }
            }

            hit = best;

            return found;
        }

        private static bool SphereRay(in Vector3 o, in Vector3 d, in Sphere sphere, out float t)
        {
            Vector3 oc = o - sphere.Center;
            float b = 2.0f * Vector3.Dot(oc, d);
            float c = Vector3.Dot(oc, oc) - (float)(sphere.Radius * sphere.Radius);
            float discriminant = b * b - 4 * c;

            if (discriminant < 0)
            {
                t = 0;
                return false;
            }
            float sqrt = (float)Math.Sqrt(discriminant);
            float t1 = (-b - sqrt) / 2.0f;
            float t2 = (-b + sqrt) / 2.0f;

            t = (t1 > 0f) ? t1 : ((t2 > 0f) ? t2 : 0f);
            if (t <= 0f) { return false; }
            return true;
        }

        private static bool TriangleRay(in Vector3 o, in Vector3 d, in Triangle tri, out float t)
        {
            const float EPS = 1e-8f;

            Vector3 pvec = Vector3.Cross(d, tri.EdgeAC);
            float det = Vector3.Dot(tri.EdgeAB, pvec);

            if (det > -EPS && det < EPS) { t = 0f; return false; }

            float invDet = 1f / det;
            Vector3 tvec = o - tri.A;

            float u = Vector3.Dot(tvec, pvec) * invDet;
            if (u < 0f || u > 1f) { t = 0f; return false; }

            Vector3 qvec = Vector3.Cross(tvec, tri.EdgeAB);
            float v = Vector3.Dot(d, qvec) * invDet;
            if (v < 0f || u + v > 1f) { t = 0f; return false; }

            t = Vector3.Dot(tri.EdgeAC, qvec) * invDet;
            return t > EPS;
        }
    }
}
