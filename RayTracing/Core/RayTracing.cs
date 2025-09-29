using RayTracing.Objects;
using RayTracing.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Core
{
    public class RayTracing
    {
        Random rnd = new Random();
        const float kEps = 1e-4f;
        public void Render(RenderTarget target, Scene Scene)
        {
            Parallel.For(0, target.Height, y =>
            {
                var py = 1 - 2 * ((y + 0.5f) / target.Height);
                for (int x = 0; x < target.Width; x++)
                {
                    int index = y * target.Width + x;

                    Vector2 pixel = new((2 * ((x + 0.5f) / target.Width) - 1) * (target.Width / (float)target.Height), py);

                    var (o, d) = CreateEyeRay(Scene.Camera, pixel);
                    target.ColourBuffer[index] = ComputeColor(Scene, o, d);
                }
            });
        }

        private static (Vector3 o, Vector3 d) CreateEyeRay(Camera camera, Vector2 pixel)
        {
            Vector3 f = Vector3.Normalize(camera.LookAt - camera.Position);
            Vector3 r = Vector3.Normalize(Vector3.Cross(camera.Up, f));
            Vector3 u = Vector3.Normalize(Vector3.Cross(r, f));
            float scale = (float)Math.Tan(camera.Fov * MathF.PI / 180f / 2);
            float beta = scale * pixel.Y;
            float omega = scale * pixel.X;

            Vector3 d = Vector3.Normalize(f + beta * u + omega * r);

            return (camera.Position, d);
        }

        private static HitPoint? FindClosestHitPoint(Scene s, Vector3 o, Vector3 d)
        {
            List<HitPoint> hits = [];
            foreach (var sphere in s.Spheres)
            {
                hits.AddRange(SphereRay(o, d, sphere));
            }

            foreach (var triangle in s.Triangles)
            {
                hits.AddRange(TriangleRay(o, d, triangle));
            }

            if (hits.Count > 0)
            {
                hits.Sort((a, b) => a.Distance.CompareTo(b.Distance));
                return hits[0];
            }
            return null;
        }

        private static List<HitPoint> SphereRay(Vector3 o, Vector3 d, Sphere sphere)
        {
            List<HitPoint> hits = [];
            Vector3 oc = o - sphere.Center;
            float b = 2.0f * Vector3.Dot(oc, d);
            float c = Vector3.Dot(oc, oc) - (float)(sphere.Radius * sphere.Radius);
            float discriminant = b * b - 4 * c;

            if (discriminant < 0)
                return [];

            float t1 = (-b - (float)Math.Sqrt(discriminant)) / 2.0f;
            float t2 = (-b + (float)Math.Sqrt(discriminant)) / 2.0f;

            if (t1 > 0)
                hits.Add(GetHitPoint(o, d, t1, sphere));
            if (t2 > 0)
                hits.Add(GetHitPoint(o, d, t2, sphere));
            return hits;
        }

        private static List<HitPoint> TriangleRay(Vector3 o, Vector3 d, Triangle triangle)
        {
            List<HitPoint> hits = [];
            Vector3 edgeAB = triangle.B - triangle.A;
            Vector3 edgeAC = triangle.C - triangle.A;

            Vector3 normalVector = Vector3.Cross(edgeAB, edgeAC);
            Vector3 ao = o - triangle.A;
            Vector3 dao = Vector3.Cross(ao, d);

            float determinant = -Vector3.Dot(d, normalVector);
            if (MathF.Abs(determinant) < 1e-8f) return hits;
            float invDet = 1f / determinant;

            float dst = Vector3.Dot(ao, normalVector) * invDet;
            float u = Vector3.Dot(edgeAC, dao) * invDet;
            float v = -Vector3.Dot(edgeAB, dao) * invDet;
            float w = 1 - u - v;

            if (dst > 1e-8f && u >= 0 && v >= 0 && w >= 0)
            {
                Vector3 hitPoint = o + dst * d;
                Vector3 normal = Vector3.Normalize(normalVector);
                hits.Add(new HitPoint { DidHit = true, Distance = dst, Point = hitPoint, Material = triangle.Material, Normal = normal });
            }

            return hits;
        }

        private static HitPoint GetHitPoint(Vector3 o, Vector3 d, float t, Sphere sphere)
        {
            Vector3 hitPoint = o + t * d;
            Vector3 normal = Vector3.Normalize(hitPoint - sphere.Center);
            return new HitPoint { DidHit = true, Distance = t, Point = hitPoint, Material = sphere.Material, Normal = normal };
        }


        private Vector3 ComputeColor(Scene scene, Vector3 o, Vector3 d)
        {
            var hit = FindClosestHitPoint(scene, o, d);
            float p = 0.007f;
            if (hit == null) return Vector3.Zero;

            if (rnd.NextDouble() < p)
            {
                return hit.Material.Emission;
            }

            var rndD = RandomDirection(hit.Normal);
            var nudge = kEps * hit.Normal;
            var newOrigin = hit.Point + nudge;

            var first = (float)(2 * Math.PI / (1 - p));
            var second = Math.Max(0, Vector3.Dot(rndD, hit.Normal));
            var brdf = BRDF(d, rndD, hit);
            var recursion = ComputeColor(scene, newOrigin, rndD);
            return hit.Material.Emission + first * second * Vector3.Multiply(brdf, recursion);
        }

        private static Vector3 BRDF(Vector3 incoming, Vector3 outgoing, HitPoint hit)
        {
            var dRef = Vector3.Reflect(incoming, hit.Normal);

            var diffuse = hit.Material.Diffuse * (float)(1 / Math.PI);

            if (Vector3.Dot(outgoing, dRef) > 1f - hit.Material.SpecularDistance)
            {
                return diffuse + hit.Material.Specular * 20;
            }

            return diffuse;
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
    }
}
