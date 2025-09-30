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
using RayTracing.Core;

namespace RayTracing.Core
{
    public class RayTracer
    {
        public int SamplesPerPixel { get; set; } = 4;
        public int MaxRayBounces { get; set; } = 8;
        public int TileSize { get; set; } = 1;
        public float BounceChance { get; set; } = 0.25f;
        private const float InvPi = 1f / MathF.PI;

        Random rnd = new Random();
        const float kEps = 1e-2f;

        public BVHNode BVHTree;

        public void Render(RenderTarget target, Scene scene)
        {
            BuildBVH(scene);
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
            int tilesX = (width + TileSize - 1) / TileSize;
            int tilesY = (height + TileSize - 1) / TileSize;
            var threadRng = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));
            Parallel.For(0, tilesX * tilesY, tileIdx =>
            {
                int tileY = tileIdx / tilesX;
                int tileX = tileIdx % tilesX;
                int startX = tileX * TileSize;
                int startY = tileY * TileSize;
                int endX = Math.Min(startX + TileSize, width);
                int endY = Math.Min(startY + TileSize, height);
                var rng = threadRng.Value;
                for (int y = startY; y < endY; y++)
                {
                    for (int x = startX; x < endX; x++)
                    {
                        int i = y * width + x;
                        Vector3 color = Vector3.Zero;
                        for (int s = 0; s < SamplesPerPixel; s++)
                        {
                            float jitterX = (float)rng.NextDouble();
                            float jitterY = (float)rng.NextDouble();
                            float py = 1 - 2 * ((y + jitterY) * invH);
                            float px = (2 * ((x + jitterX) * invW) - 1) * aspect;
                            Vector3 d = f + (py * scale) * u + (px * scale) * r;
                            d = Vector3.Normalize(d);
                            color += ComputeColor(scene, cam.Position, d);
                        }
                        color /= SamplesPerPixel;
                        target.ColourBuffer[i] = color;
                    }
                }
            });
        }

        public void BuildBVH(Scene scene)
        {
            BVHTree = BVH.Build(scene.Triangles, scene.Spheres);
        }

        private bool FindClosestHitPoint(in Scene s, in Vector3 o, in Vector3 d, out HitPoint? hit)
        {
            // Use BVH for intersection
            return FindClosestHitPointBVH(BVHTree, o, d, out hit);
        }

        private bool SphereRay(in Vector3 o, in Vector3 d, in Sphere sphere, out float t)
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

        private bool TriangleRay(in Vector3 o, in Vector3 d, in Triangle tri, out float t)
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



        private Vector3 ComputeColor(Scene scene, Vector3 o, Vector3 d, int depth = 0)
        {
            if (!FindClosestHitPoint(scene, o, d, out HitPoint? hit) || hit == null) return Vector3.Zero;

            if ((BounceChance > 0f && (float)rnd.NextDouble() < BounceChance))
            {
                return hit.Material.Emission;
            }

            var rndD = RandomDirection(hit.Normal);
            var nudge = kEps * hit.Normal;
            var newOrigin = hit.Point + nudge;

            var first = (float)(2 * Math.PI / (1 - BounceChance));
            var second = Math.Max(0, Vector3.Dot(rndD, hit.Normal));
            var brdf = BRDF(d, rndD, hit);
            var recursion = ComputeColor(scene, newOrigin, rndD, depth + 1);
            return hit.Material.Emission + first * second * Vector3.Multiply(brdf, recursion);
        }

        private static Vector3 BRDF(Vector3 incoming, Vector3 outgoing, HitPoint hit)
        {
            var diffuse = hit.Material.Diffuse * InvPi;

            Vector3 n = Vector3.Normalize(hit.Normal);
            Vector3 wi = Vector3.Normalize(incoming);
            Vector3 wo = Vector3.Normalize(outgoing);
            
            Vector3 dr = Vector3.Reflect(wi, n);

            if (Vector3.Dot(wo, dr) > 1 - 0.005)
            {
                return diffuse + 10 * hit.Material.Specular;
            }

            return diffuse;
        }
        private static Vector3 BRDFBlinn(Vector3 incoming, Vector3 outgoing, HitPoint hit)
        {
            var diffuse = hit.Material.Diffuse * (float)(1 / Math.PI);

            // Blinn-Phong Specular
            Vector3 viewDir = -incoming; // assuming incoming is from camera to surface
            Vector3 halfDir = Vector3.Normalize(viewDir + outgoing);
            float specAngle = Math.Max(Vector3.Dot(hit.Normal, halfDir), 0f);
            float shininess = 1f / Math.Max(hit.Material.SpecularDistance, 0.001f); // higher shininess = smaller highlight
            float specularStrength = MathF.Pow(specAngle, shininess);

            var specular = hit.Material.Specular * specularStrength;

            return diffuse + specular;
        }

        private Vector3 RandomDirection(Vector3 normal)
        {
            Vector3 rndD = new Vector3();
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
            return (float)((rnd.NextDouble() * 2) - 1);
        }

        private bool FindClosestHitPointBVH(BVHNode node, in Vector3 o, in Vector3 d, out HitPoint? hit)
        {
            hit = null;
            float closest = float.PositiveInfinity;
            HitPoint? best = null;
            if (node == null) return false;
            if (!node.Bounds.Intersect(o, d, out float tmin, out float tmax)) return false;
            if (node.IsLeaf)
            {
                foreach (var sphere in node.Spheres ?? new List<Sphere>())
                {
                    if (SphereRay(o, d, sphere, out var dist))
                    {
                        if (dist < closest)
                        {
                            closest = dist;
                            Vector3 p = o + dist * d;
                            var normal = Vector3.Normalize(p - sphere.Center);
                            best = new HitPoint { DidHit = true, Material = sphere.Material, Distance = dist, Point = p, Normal = normal };
                        }
                    }
                }
                foreach (var triangle in node.Triangles ?? new List<Triangle>())
                {
                    if (Vector3.Dot(d, triangle.NormalUnit) >= 0f) continue;
                    if (TriangleRay(o, d, triangle, out var dist))
                    {
                        if (dist < closest)
                        {
                            closest = dist;
                            best = new HitPoint { DidHit = true, Material = triangle.Material, Distance = dist, Point = o + dist * d, Normal = triangle.NormalUnit };
                        }
                    }
                }
                hit = best;
                return best != null;
            }
            bool foundLeft = FindClosestHitPointBVH(node.Left, o, d, out HitPoint? hitLeft);
            bool foundRight = FindClosestHitPointBVH(node.Right, o, d, out HitPoint? hitRight);
            if (foundLeft && (!foundRight || hitLeft.Distance < hitRight.Distance))
            {
                hit = hitLeft;
                return true;
            }
            if (foundRight)
            {
                hit = hitRight;
                return true;
            }
            return false;
        }
    }
}
