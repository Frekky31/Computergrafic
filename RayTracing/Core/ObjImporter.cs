using RayTracing.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Core
{
    public static class ObjImporter
    {
        public static (List<Triangle> triangles, List<Vector3> positions) LoadObj(string path, Vector3 color)
        {
            var positionsRaw = new List<Vector3>();   // OBJ "v"
            var normalsRaw = new List<Vector3>();   // OBJ "vn"
            var uvsRaw = new List<Vector2>();   // OBJ "vt" (optional)
            var triangles = new List<Triangle>();

            // Axis remap: Blender (X right, Y forward, Z up) -> Engine (X right, Y up, Z forward)
            static Vector3 TransformPos(Vector3 v) => new Vector3(v.X, v.Z, -v.Y);
            static Vector3 TransformDir(Vector3 n) => Vector3.Normalize(new Vector3(n.X, n.Z, -n.Y)); // for normals

            // Determine if transform flips handedness (reflection) to fix winding automatically
            Vector3 tx = TransformDir(Vector3.UnitX);
            Vector3 ty = TransformDir(Vector3.UnitY);
            Vector3 tz = TransformDir(Vector3.UnitZ);
            bool flipWinding = Vector3.Dot(Vector3.Cross(tx, ty), tz) < 0f;  // true for (x, z, -y)

            var trianglesOutPositions = new List<Vector3>(); // transformed positions list to return (debug/usage)

            var inv = CultureInfo.InvariantCulture;
            foreach (var raw in File.ReadLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line[0] == '#') continue;
                Vector3 color2 = color * 0.5f;

                if (line.StartsWith("v ", StringComparison.Ordinal))
                {
                    var sp = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    float x = float.Parse(sp[1], inv);
                    float y = float.Parse(sp[2], inv);
                    float z = float.Parse(sp[3], inv);
                    positionsRaw.Add(new Vector3(x, y, z));
                }
                else if (line.StartsWith("vt ", StringComparison.Ordinal))
                {
                    var sp = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    float u = float.Parse(sp[1], inv);
                    float v = float.Parse(sp.Length > 2 ? sp[2] : "0", inv);
                    uvsRaw.Add(new Vector2(u, v));
                }
                else if (line.StartsWith("vn ", StringComparison.Ordinal))
                {
                    var sp = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    float nx = float.Parse(sp[1], inv);
                    float ny = float.Parse(sp[2], inv);
                    float nz = float.Parse(sp[3], inv);
                    normalsRaw.Add(new Vector3(nx, ny, nz));
                }
                else if (line.StartsWith("f ", StringComparison.Ordinal))
                {
                    // Parse all vertex specs on this face: v, v/vt, v//vn, v/vt/vn
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    int count = parts.Length - 1;
                    if (count < 3) continue;

                    // Collect indices for the polygon
                    var vIdx = new int[count];
                    var vtIdx = new int[count];
                    var vnIdx = new int[count];

                    for (int i = 0; i < count; i++)
                    {
                        var comp = parts[i + 1].Split('/'); // up to 3 fields
                                                            // OBJ indices are 1-based; negative means relative to end
                        vIdx[i] = ResolveIndex(ParseInt(comp[0]), positionsRaw.Count);
                        vtIdx[i] = (comp.Length > 1 && comp[1].Length > 0) ? ResolveIndex(ParseInt(comp[1]), uvsRaw.Count) : -1;
                        vnIdx[i] = (comp.Length > 2 && comp[2].Length > 0) ? ResolveIndex(ParseInt(comp[2]), normalsRaw.Count) : -1;
                    }
                    
                    int counter = 0;
                    // Triangulate as a fan: (0, i, i+1)
                    for (int i = 1; i < count - 1; i++)
                    {
                        int ia = vIdx[0], ib = vIdx[i], ic = vIdx[i + 1];

                        // Transform positions from Blender to engine
                        Vector3 A = TransformPos(positionsRaw[ia]);
                        Vector3 B = TransformPos(positionsRaw[ib]);
                        Vector3 C = TransformPos(positionsRaw[ic]);

                        // Fix winding if the transform flips handedness
                        if (flipWinding)
                            (B, C) = (C, B);

                        // Optional: use per-vertex normals if present to ensure consistent facing
                        // (Not needed for flat shading, but helpful if your Triangle ctor uses vn)
                        // Vector3 nA = (vnIdx[0] >= 0)      ? TransformDir(normalsRaw[vnIdx[0]])      : Vector3.Zero;
                        // Vector3 nB = (vnIdx[i] >= 0)      ? TransformDir(normalsRaw[vnIdx[i]])      : Vector3.Zero;
                        // Vector3 nC = (vnIdx[i+1] >= 0)    ? TransformDir(normalsRaw[vnIdx[i+1]])    : Vector3.Zero;

                        triangles.Add(new Triangle(A, B, C, counter % 2 == 0 ? color : color2));
                        counter++;
                        // If you still want a transformed vertex list to return:
                        trianglesOutPositions.Add(A);
                        trianglesOutPositions.Add(B);
                        trianglesOutPositions.Add(C);
                    }
                }
            }

            return (triangles, trianglesOutPositions);

            // ---- helpers ----
            static int ParseInt(string s) => int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
            static int ResolveIndex(int idx, int count)
            {
                // OBJ: positive = 1-based, negative = relative to end, zero is invalid
                return (idx > 0) ? (idx - 1) : (count + idx);
            }
        }
    }
}
