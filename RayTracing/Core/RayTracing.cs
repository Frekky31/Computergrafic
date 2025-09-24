using RayTracing.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Core
{
    public class RayTracing
    {
        public static void Render(RenderTarget target, Scene Scene)
        {
            Parallel.For(0, target.Height, y =>
            {
                for (int x = 0; x < target.Width; x++)
                {
                    int index = y * target.Width + x;

                    Vector2 pixel = new((2 * ((x + 0.5f) / target.Width) - 1) * (target.Width / (float)target.Height), 1 - 2 * ((y + 0.5f) / target.Height));

                    var (o, d) = CreateEyeRay(Scene.Camera, pixel);
                    var hit = FindClosestHitPoint(Scene, o, d);
                    if (hit != null && hit.DidHit)
                    {
                        target.ColourBuffer[index] = ComputeColor(Scene, o, d, hit);
                    }
                    else
                    {
                        target.ColourBuffer[index] = new Vector3(0f, 0f, 0f);
                    }
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
                hits.AddRange(SphereRay(o,d,sphere));
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
            Vector3 oc = o - sphere.center;
            float b = 2.0f * Vector3.Dot(oc, d);
            float c = Vector3.Dot(oc, oc) - (float)(sphere.radius * sphere.radius);
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
            // For left-handed, Z-up: cross product order and up direction must be considered.
            // Z-up: up = (0, 0, 1), left-handed: cross order is reversed.
            Vector3 normalVector = Vector3.Cross(edgeAB, edgeAC); // reverse order for left-handed
            Vector3 ao = o - triangle.A;
            Vector3 dao = Vector3.Cross(ao, d); // reverse order for left-handed

            float determinant = -Vector3.Dot(normalVector, d); // no sign flip for left-handed
            if (Math.Abs(determinant) < 1e-8f)
                return hits;

            float invDet = 1.0f / determinant;

            float dst = Vector3.Dot(normalVector, ao) * invDet;
            float u = Vector3.Dot(Vector3.Cross(d, edgeAC), ao) * invDet;
            float v = Vector3.Dot(Vector3.Cross(edgeAB, d), ao) * invDet;
            float w = 1 - u - v;

            if (determinant > 1e-8f && dst > 1e-8f && u >= 0 && v >= 0 && w >= 0)
            {
                Vector3 hitPoint = o + dst * d;
                Vector3 normal = Vector3.Normalize(normalVector);
                hits.Add(new HitPoint { DidHit = true, Distance = dst, Point = hitPoint, Color = triangle.Color, Normal = normal });
            }

            return hits;
        }

        private static HitPoint GetHitPoint(Vector3 o, Vector3 d, float t, Sphere sphere)
        {
            Vector3 hitPoint = o + t * d;
            Vector3 normal = Vector3.Normalize(hitPoint - sphere.center);
            return new HitPoint { DidHit = true, Distance = t, Point = hitPoint, Color = sphere.color, Normal = normal };
        }


        private static Vector3 ComputeColor(Scene scene, Vector3 o, Vector3 d, HitPoint hit)
        {
            return hit.Color;
        }
    }
}
