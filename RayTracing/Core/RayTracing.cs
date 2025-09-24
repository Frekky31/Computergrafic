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
                Vector3 oc = o - sphere.center;
                float b = 2.0f * Vector3.Dot(oc, d);
                float c = Vector3.Dot(oc, oc) - (float)(sphere.radius * sphere.radius);
                float discriminant = b * b - 4 * c;

                if (discriminant < 0)
                    continue;

                float t1 = (-b - (float)Math.Sqrt(discriminant)) / 2.0f;
                float t2 = (-b + (float)Math.Sqrt(discriminant)) / 2.0f;

                if (t1 > 0)
                    hits.Add(GetHitPoint(o, d, t1, sphere));
                if (t2 > 0)
                    hits.Add(GetHitPoint(o, d, t2, sphere));
            }

            if (hits.Count > 0)
            {
                hits.Sort((a, b) => a.Distance.CompareTo(b.Distance));
                return hits[0];
            }
            return null;
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
