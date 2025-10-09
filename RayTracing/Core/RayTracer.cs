using RayTracing.Objects;
using RayTracing.Scenes;
using System.Numerics;
using RayTracing.Texture;
using System.Collections.Concurrent;

namespace RayTracing.Core
{
    public class RayTracer
    {
        private const float kEps = 1e-4f;
        private Vector3 c_f;
        private Vector3 c_r;
        private Vector3 c_u;
        private float c_scale;
        private const float InvPi = 1f / MathF.PI;
        public BVHNode? BVH;

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
            BuildBVH(Scene);
            c_f = Vector3.Normalize(Scene.Camera.LookAt - Scene.Camera.Position);
            c_r = Vector3.Normalize(Vector3.Cross(Scene.Camera.Up, c_f));
            c_u = Vector3.Normalize(Vector3.Cross(c_r, c_f));
            c_scale = (float)MathF.Tan(Scene.Camera.Fov * MathF.PI / 180f / 2);

            int totalRays = target.Width * target.Height;
            int processedRays = 0;
            int lastReported = 0;

            Parallel.ForEach(Partitioner.Create(0, target.Height), (range, state) =>
            {
                Vector3[] localBuffer = new Vector3[target.Width];

                for (int y = range.Item1; y < range.Item2; y++)
                {
                    var py = 1 - 2 * ((y + 0.5f) / target.Height);
                    for (int x = 0; x < target.Width; x++)
                    {
                        int index = y * target.Width + x;
                        Vector3 sampleBuffer = BackgroundColor;

                        float pixelX = (2 * ((x + 0.5f) / target.Width) - 1) * (target.Width / (float)target.Height);
                        float pixelY = py;

                        float beta = c_scale * pixelY;
                        float omega = c_scale * pixelX;

                        Vector3 d = Vector3.Normalize(c_f + beta * c_u + omega * c_r);
                        for (int i = 0; i < SamplesPerPixel; i++)
                        {
                            sampleBuffer += ComputeColorBRDF(Scene, Scene.Camera.Position, d, 0);
                        }

                        localBuffer[x] = sampleBuffer / SamplesPerPixel;

                        int current = Interlocked.Increment(ref processedRays);
                        if (ProgressCallback != null && (current - lastReported > totalRays / 100 || current == totalRays))
                        {
                            int prev = Interlocked.Exchange(ref lastReported, current);
                            if (current - prev > 0)
                                ProgressCallback(current, totalRays);
                        }
                    }

                    for (int x = 0; x < target.Width; x++)
                    {
                        target.ColourBuffer[y * target.Width + x] = localBuffer[x];
                    }
                }
            });
        }

        public void BuildBVH(Scene scene)
        {
            var allTriangles = new List<Triangle>();
            var allSpheres = new List<Sphere>();
            foreach (var obj in scene.Triangles)
            {
                allTriangles.AddRange(obj.GetTriangles().ToArray());
            }
            foreach (var obj in scene.Spheres)
            {
                allSpheres.AddRange(obj.GetSpheres().ToArray());
            }
            BVH = BVHTree.Build(scene.Triangles, scene.Spheres);
        }

        /*
         * 
         * BRDF Path Tracing and Color Computation
         * 
         */
        private Vector3 ComputeColorBRDF(Scene scene, Vector3 o, Vector3 d, int depth)
        {
            if (!FindClosestHitPointBVH(BVH, o, d, out HitPoint? optionalHit) || !optionalHit.HasValue) return BackgroundColor;

            var hit = optionalHit.Value;
            Vector3 emission = hit.Material.Emission;
            if (hit.Material.HasTexture && hit.RenderObject is Sphere sphere)
            {
                Vector2 uv = Sphere.GetSphereUV(hit.Point, sphere);
                emission = Material.SampleTexture(hit.Material, uv) * hit.Material.Emission;
            }

            if (threadRng.Value!.NextDouble() < Probability || depth >= MaxDepth)
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

        private Vector3 ComputeColor(Scene scene, Vector3 o, Vector3 d, int depth)
        {
            if (FindClosestHitPoint(scene, o, d, out HitPoint? optionalHit) && optionalHit.HasValue)
            {
                var hit = optionalHit.Value;
                if (hit.Material.HasTexture && hit.RenderObject is Sphere sphere)
                {
                    Vector2 uv = Sphere.GetSphereUV(hit.Point, sphere);
                    return Material.SampleTexture(hit.Material, uv) * hit.Material.Diffuse;
                }
                return hit.Material.Diffuse;
            }

            return BackgroundColor;
        }

        private static Vector3 BRDF(Vector3 incoming, Vector3 outgoing, HitPoint hit)
        {
            Vector3 texColor = Vector3.One;
            if (hit.Material.HasTexture && hit.RenderObject is Sphere sphere)
            {
                Vector2 uv = Sphere.GetSphereUV(hit.Point, sphere);
                texColor = Material.SampleTexture(hit.Material, uv);
            }

            Vector3 diffuse = hit.Material.Diffuse * texColor * InvPi;
            Vector3 specular = hit.Material.Specular * texColor * InvPi;

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

        private static Vector3 RandomDirection(Vector3 normal)
        {
            Vector3 rndD;
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

        public static float NextFloat()
        {
            return (float)((threadRng.Value!.NextDouble() * 2) - 1);
        }

        /*
         * 
         * Hit Detection
         * 
         */

        private static bool SphereRay(Vector3 o, Vector3 d, Sphere sphere, out float t)
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
        private static bool TriangleRay(Vector3 o, Vector3 d, Triangle triangle, out float t)
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

        private static bool FindClosestHitPointBVH(BVHNode? node, in Vector3 o, in Vector3 d, out HitPoint? hit)
        {
            hit = null;
            float closest = float.PositiveInfinity;
            HitPoint? best = null;
            if (node == null) return false;
            if (!node.Bounds.Intersect(o, d, out _, out _)) return false;
            if (node.IsLeaf)
            {
                return ObjectHitpoint(node, o, d, out hit, ref closest, ref best);
            }
            bool foundLeft = FindClosestHitPointBVH(node.Left, o, d, out HitPoint? hitLeft);
            bool foundRight = FindClosestHitPointBVH(node.Right, o, d, out HitPoint? hitRight);
            if (foundLeft && (!foundRight || hitLeft?.Distance < hitRight?.Distance))
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

        private static bool ObjectHitpoint(BVHNode node, Vector3 o, Vector3 d, out HitPoint? hit, ref float closest, ref HitPoint? best)
        {
            foreach (var sphere in node.Spheres ?? [])
            {
                if (SphereRay(o, d, sphere, out var dist) && dist < closest)
                {
                    closest = dist;
                    Vector3 p = o + dist * d;
                    var normal = Vector3.Normalize(p - sphere.Center);
                    best = new HitPoint { DidHit = true, Material = sphere.Material, Distance = dist, Point = p, Normal = normal, RenderObject = sphere };
                }
            }
            foreach (var triangle in node.Triangles ?? [])
            {
                if (Vector3.Dot(d, triangle.NormalUnit) >= 0f) continue;
                if (TriangleRay(o, d, triangle, out var dist) && dist < closest)
                {
                    closest = dist;
                    best = new HitPoint { DidHit = true, Material = triangle.Material, Distance = dist, Point = o + dist * d, Normal = triangle.NormalUnit, RenderObject = triangle };
                }
            }
            hit = best;
            return best != null;
        }


        // Version without BVH

        private static bool FindClosestHitPoint(in Scene s, in Vector3 o, in Vector3 d, out HitPoint? hit)
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
    }
}
