using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Objects
{
    public class Mesh : RenderObject
    {
        public Mesh(Span<Triangle> triangles, Span<Vector3> vertices)
        {
            Triangles = triangles.ToArray();
            Vertices = vertices.ToArray();
        }
        public Triangle[] Triangles;
        public Vector3[] Vertices;

        public override void Rotate(Quaternion rotation)
        {
            // Rotate all vertices
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = Vector3.Transform(Vertices[i], rotation);
            }

            // Update triangles to use rotated vertices
            foreach (var t in Triangles)
            {
                t.A = Vector3.Transform(t.A, rotation);
                t.B = Vector3.Transform(t.B, rotation);
                t.C = Vector3.Transform(t.C, rotation);
                t.EdgeAB = t.B - t.A;
                t.EdgeAC = t.C - t.A;
                t.NormalUnit = Vector3.Normalize(Vector3.Cross(t.EdgeAB, t.EdgeAC));
            }
        }

        public override void Move(Vector3 translation)
        {
            // Move all vertices
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] += translation;
            }
            // Update triangles to use moved vertices
            foreach (var t in Triangles)
            {
                t.A += translation;
                t.B += translation;
                t.C += translation;
                t.EdgeAB = t.B - t.A;
                t.EdgeAC = t.C - t.A;
                t.NormalUnit = Vector3.Normalize(Vector3.Cross(t.EdgeAB, t.EdgeAC));
            }
        }

        public override void Scale(float scale)
        {
            // Scale all vertices from the origin
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] *= scale;
            }
            // Update triangles to use scaled vertices
            foreach (var t in Triangles)
            {
                t.A *= scale;
                t.B *= scale;
                t.C *= scale;
                t.EdgeAB = t.B - t.A;
                t.EdgeAC = t.C - t.A;
                t.NormalUnit = Vector3.Normalize(Vector3.Cross(t.EdgeAB, t.EdgeAC));
            }
        }

        public override Span<Triangle> GetTriangles()
        {
            return Triangles;
        }

        public override Span<Sphere> GetSpheres()
        {
            return Vertices.Select(v => new Sphere(0.01f, v, new Vector3(1, 1, 1))).ToArray();
        }
    }
}
