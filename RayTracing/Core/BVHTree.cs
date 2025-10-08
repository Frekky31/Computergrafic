using RayTracing.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Core
{
    public static class BVHTree
    {
        public static BVHNode? Build(List<Triangle> triangles, List<Sphere> spheres, int depth = 0)
        {
            int triCount = triangles.Count;
            int sphCount = spheres.Count;
            if (triCount + sphCount == 0) return null;

            var allBoxes = new List<BoundingBox>(triCount + sphCount);
            foreach (var t in triangles)
            {
                var min = Vector3.Min(Vector3.Min(t.A, t.B), t.C);
                var max = Vector3.Max(Vector3.Max(t.A, t.B), t.C);
                allBoxes.Add(new BoundingBox(min, max));
            }
            foreach (var s in spheres)
            {
                var min = s.Center - new Vector3(s.Radius);
                var max = s.Center + new Vector3(s.Radius);
                allBoxes.Add(new BoundingBox(min, max));
            }

            BoundingBox bounds = allBoxes[0];
            for (int i = 1; i < allBoxes.Count; i++)
                bounds = BoundingBox.Union(bounds, allBoxes[i]);

            if (triCount + sphCount <= 4 || depth > 16)
            {
                return new BVHNode
                {
                    Bounds = bounds,
                    Triangles = triCount > 0 ? [.. triangles] : null,
                    Spheres = sphCount > 0 ? [.. spheres] : null
                };
            }

            Vector3 size = bounds.Max - bounds.Min;
            int axis = size.X > size.Y && size.X > size.Z ? 0 : (size.Y > size.Z ? 1 : 2);
            float mid = 0.5f * (bounds.Min[axis] + bounds.Max[axis]);

            var leftTris = new List<Triangle>();
            var rightTris = new List<Triangle>();
            foreach (var t in triangles)
            {
                float centroid = (t.A[axis] + t.B[axis] + t.C[axis]) / 3f;
                if (centroid < mid) leftTris.Add(t); else rightTris.Add(t);
            }

            var leftSph = new List<Sphere>();
            var rightSph = new List<Sphere>();
            foreach (var s in spheres)
            {
                float centroid = s.Center[axis];
                if (centroid < mid) leftSph.Add(s); else rightSph.Add(s);
            }

            if (leftTris.Count == triCount && leftSph.Count == sphCount)
            {
                int halfTris = triCount / 2;
                int halfSph = sphCount / 2;
                leftTris = [.. triangles.Take(halfTris)];
                rightTris = [.. triangles.Skip(halfTris)];
                leftSph = [.. spheres.Take(halfSph)];
                rightSph = [.. spheres.Skip(halfSph)];
            }

            return new BVHNode
            {
                Bounds = bounds,
                Left = Build(leftTris, leftSph, depth + 1),
                Right = Build(rightTris, rightSph, depth + 1)
            };
        }
    }

    public class BVHNode
    {
        public BoundingBox Bounds;
        public BVHNode? Left, Right = default;
        public List<Triangle>? Triangles = [];
        public List<Sphere>? Spheres = [];

        public bool IsLeaf => (Triangles != null && Triangles.Count > 0) || (Spheres != null && Spheres.Count > 0);
    }

    public struct BoundingBox(Vector3 min, Vector3 max)
    {
        public Vector3 Min { get; set; } = min;
        public Vector3 Max { get; set; } = max;

        public readonly bool Intersect(Vector3 o, Vector3 d, out float tmin, out float tmax)
        {
            tmin = float.MinValue; tmax = float.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                float invD = 1f / d[i];
                float t0 = (Min[i] - o[i]) * invD;
                float t1 = (Max[i] - o[i]) * invD;
                if (invD < 0f) (t0, t1) = (t1, t0);
                tmin = Math.Max(tmin, t0);
                tmax = Math.Min(tmax, t1);
                if (tmax < tmin) return false;
            }
            return true;
        }

        public static BoundingBox Union(BoundingBox a, BoundingBox b)
        {
            return new BoundingBox(Vector3.Min(a.Min, b.Min), Vector3.Max(a.Max, b.Max));
        }
    }
}
