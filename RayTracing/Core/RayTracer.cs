using RayTracing.Objects;
using RayTracing.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using RayTracing.Texture;

namespace RayTracing.Core
{
    public class RayTracer
    {
        private const float kEps = 1e-4f;
        private Vector3 c_f;
        private Vector3 c_r;
        private Vector3 c_u;
        private float c_scale;
        const float InvPi = 1f / MathF.PI;

        public Vector3 BackgroundColor { get; set; } = new(0.1f, 0.1f, 0.1f);
        public int MaxDepth { get; set; } = 5;
        public int SamplesPerPixel { get; set; } = 1;
        public float Probability { get; set; } = 0.8f;

        private static readonly ThreadLocal<Random> threadRng = new(() => new Random(Guid.NewGuid().GetHashCode()));

        public Action<int, int>? ProgressCallback { get; set; }

        public RayTracer()
        {
        }

        public void Render(RenderTarget target, Scene Scene)
        {
            c_f = Vector3.Normalize(Scene.Camera.LookAt - Scene.Camera.Position);
            c_r = Vector3.Normalize(Vector3.Cross(Scene.Camera.Up, c_f));
            c_u = Vector3.Normalize(Vector3.Cross(c_r, c_f));
            c_scale = (float)MathF.Tan(Scene.Camera.Fov * MathF.PI / 180f / 2);

            int totalRays = target.Width * target.Height;
            int processedRays = 0;
            int lastReported = 0;

            Parallel.For(0, target.Height, y =>
            {
                var py = 1 - 2 * ((y + 0.5f) / target.Height);
                for (int x = 0; x < target.Width; x++)
                {
                    int index = y * target.Width + x;
                    Vector3 sampleBuffer = BackgroundColor;

                    Vector2 pixel = new((2 * ((x + 0.5f) / target.Width) - 1) * (target.Width / (float)target.Height), py);

                    float beta = c_scale * pixel.Y;
                    float omega = c_scale * pixel.X;

                    Vector3 d = Vector3.Normalize(c_f + beta * c_u + omega * c_r);
                    for (int i = 0; i < SamplesPerPixel; i++)
                    {
                        sampleBuffer += ComputeColorBRDF(Scene, Scene.Camera.Position, d, 0);
                    }

                    target.ColourBuffer[index] = sampleBuffer / SamplesPerPixel;

                    // Progress reporting (every 1% or 1000 rays)
                    int current = Interlocked.Increment(ref processedRays);
                    if (ProgressCallback != null && (current - lastReported > totalRays / 100 || current == totalRays))
                    {
                        int prev = Interlocked.Exchange(ref lastReported, current);
                        if (current - prev > 0)
                            ProgressCallback(current, totalRays);
                    }
                }
            });
        }

        private bool FindClosestHitPoint(in Scene s, in Vector3 o, in Vector3 d, out HitPoint? hit)
        {
            float closest = float.PositiveInfinity;
            HitPoint? best = default;
            bool found = false;

            foreach (var sphere in s.Spheres)
            {
                if (SphereRay(o, d, sphere, out var dist) && dist < closest)
                {
                    closest = dist;
                    Vector3 p = o + dist * d;
                    var normal = Vector3.Normalize(p - sphere.Center);
                    best = new HitPoint { DidHit = true, Material = sphere.Material, Distance = dist, Point = p, Normal = normal, RenderObject = sphere };
                    found = true;
                }
            }

            foreach (var triangle in s.Triangles)
            {
                if (Vector3.Dot(d, triangle.NormalUnit) >= 0f)
                    continue;

                if (TriangleRay(o, d, triangle, out var dist) && dist < closest)
                {
                    closest = dist;
                    best = new HitPoint { DidHit = true, Material = triangle.Material, Distance = dist, Point = o + dist * d, Normal = triangle.NormalUnit, RenderObject = triangle };
                    found = true;
                }
            }

            hit = best;

            return found;
        }

        private bool SphereRay(Vector3 o, Vector3 d, Sphere sphere, out float t)
        {
            Vector3 oc = o - sphere.Center;
            float b = Vector3.Dot(oc, d);
            float c = Vector3.Dot(oc, oc) - (float)(sphere.Radius * sphere.Radius);
            float discriminant = b * b - c;
            t = float.MaxValue;

            if (discriminant < 0)
                return false;

            float sqrtD = MathF.Sqrt(discriminant);
            float t1 = -b - sqrtD;
            float t2 = -b + sqrtD;

            t = (t1 > 0f) ? t1 : ((t2 > 0f) ? t2 : 0f);
            if (t <= 0f) { return false; }
            return true;
        }

        private bool TriangleRay(Vector3 o, Vector3 d, Triangle triangle, out float t)
        {
            Vector3 edgeAB = triangle.B - triangle.A;
            Vector3 edgeAC = triangle.C - triangle.A;

            Vector3 normalVector = Vector3.Cross(edgeAB, edgeAC);
            Vector3 ao = o - triangle.A;
            Vector3 dao = Vector3.Cross(ao, d);

            float determinant = -Vector3.Dot(d, normalVector);
            t = float.MaxValue;
            if (MathF.Abs(determinant) < 1e-8f) return false;
            float invDet = 1f / determinant;

            float dst = Vector3.Dot(ao, normalVector) * invDet;
            float u = Vector3.Dot(edgeAC, dao) * invDet;
            float v = -Vector3.Dot(edgeAB, dao) * invDet;
            float w = 1 - u - v;

            if (dst > 1e-8f && u >= 0 && v >= 0 && w >= 0)
            {
                t = dst;
                return true;
            }

            return false;
        }

        private Vector3 RandomDirection(Vector3 normal)
        {
            Vector3 rndD = new();
            do
            {
                rndD = new(NextFloat(), NextFloat(), NextFloat());
            } while (rndD.LengthSquared() > 1f);
            if (Vector3.Dot(rndD, normal) < 0)
            {
                rndD = -rndD;
            }
            return Vector3.Normalize(rndD);
        }

        public float NextFloat()
        {
            return (float)(threadRng.Value.NextDouble() * 2 - 1);
        }

        private Vector3 ComputeColorBRDF(Scene scene, Vector3 o, Vector3 d, int depth)
        {
            if (!FindClosestHitPoint(scene, o, d, out HitPoint? hit) || hit == null) return BackgroundColor;

            Vector3 emission = hit.Material.Emission;
            if (hit.Material.Texture != null && hit.RenderObject is Sphere sphere)
            {
                Vector2 uv = GetSphereUV(hit.Point, sphere);
                emission = SampleTexture(hit.Material, uv) * hit.Material.Emission;
            }

            if (threadRng.Value.NextDouble() < Probability || depth >= MaxDepth)
            {
                return emission;
            }

            var rndD = RandomDirection(hit.Normal);
            var nudge = kEps * hit.Normal;
            var newOrigin = hit.Point + nudge;

            var first = (float)(2 * MathF.PI / (1 - Probability));
            var second = MathF.Max(0, Vector3.Dot(rndD, hit.Normal));
            var brdf = BRDF(d, rndD, hit);
            var recursion = ComputeColorBRDF(scene, newOrigin, rndD, depth + 1);
            return emission + first * second * Vector3.Multiply(brdf, recursion);
        }

        private Vector3 ComputeColor(Scene scene, Vector3 o, Vector3 d)
        {
            if (FindClosestHitPoint(scene, o, d, out HitPoint? hit))
            {
                return hit?.Material.Diffuse ?? BackgroundColor;
            }

            return BackgroundColor;
        }

        private Vector3 BRDF(Vector3 incoming, Vector3 outgoing, HitPoint hit)
        {
            Vector3 diffuse = hit.Material.Diffuse * InvPi;
            Vector3 specular = hit.Material.Specular * InvPi;

            if ((hit.Material.Texture != null || hit.Material.ProceduralTexture != null) && hit.RenderObject is Sphere sphere)
            {
                Vector2 uv = GetSphereUV(hit.Point, sphere);
                var texColor = SampleTexture(hit.Material, uv) * InvPi;
                diffuse = texColor + diffuse;
                specular = texColor + specular;
            }

            Vector3 n = Vector3.Normalize(hit.Normal);
            Vector3 wi = Vector3.Normalize(incoming);
            Vector3 wo = Vector3.Normalize(outgoing);

            Vector3 dr = Vector3.Reflect(wi, n);

            if (Vector3.Dot(wo, dr) > 1 - hit.Material.SpecularDistance)
            {
                return diffuse + 10 * specular;
            }

            return diffuse;
        }

        private static Vector2 GetSphereUV(Vector3 point, Sphere sphere)
        {
            Vector3 p = Vector3.Normalize(point - sphere.Center);
            float u = 0.5f + (float)(MathF.Atan2(p.Z, p.X) / (2 * MathF.PI));
            float v = 0.5f - (float)(MathF.Asin(p.Y) * InvPi);
            return new Vector2(u, v);
        }

        private Vector3 SampleTexture(Material material, Vector2 uv)
        {
            if (material.ProceduralTexture != null)
                return material.ProceduralTexture(uv);

            if (material.Texture != null)
            {
                lock (material.Texture)
                {
                    int x = Math.Clamp((int)(uv.X * material.Texture.Width), 0, material.Texture.Width - 1);
                    int y = Math.Clamp((int)(uv.Y * material.Texture.Height), 0, material.Texture.Height - 1);
                    var color = material.Texture.GetPixel(x, y);
                    return new Vector3(
                        (float)MathF.Pow(color.R / 255f, 2.2f),
                        (float)MathF.Pow(color.G / 255f, 2.2f),
                        (float)MathF.Pow(color.B / 255f, 2.2f)
                    );
                }
            }

            return material.Diffuse;
        }
    }
}
