using OpenGL.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace Rasterization.Objects
{
    public static class MeshLoader
    {
        public static Mesh LoadObj(string path, Vector3 defaultColor)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var texCoords = new List<Vector2>();
            var meshVertices = new List<Vertex>();
            var tris = new List<(int A, int B, int C)>();

            foreach (var line in File.ReadLines(path))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("v "))
                {
                    var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    vertices.Add(new Vector3(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture)
                    ));
                }
                else if (trimmed.StartsWith("vn "))
                {
                    var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    normals.Add(new Vector3(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture)
                    ));
                }
                else if (trimmed.StartsWith("vt "))
                {
                    var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    texCoords.Add(new Vector2(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture)
                    ));
                }
                else if (trimmed.StartsWith("f "))
                {
                    var parts = trimmed.Substring(2).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    int[] indices = new int[3];
                    for (int i = 0; i < 3; i++)
                    {
                        var faceParts = parts[i].Split('/');
                        int vIdx = int.Parse(faceParts[0]) - 1;
                        int vtIdx = faceParts.Length > 1 && faceParts[1] != "" ? int.Parse(faceParts[1]) - 1 : -1;
                        int vnIdx = faceParts.Length > 2 ? int.Parse(faceParts[2]) - 1 : -1;

                        var position = vertices[vIdx];
                        var color = defaultColor;
                        var texCoord = vtIdx >= 0 ? texCoords[vtIdx] : Vector2.Zero;
                        var normal = vnIdx >= 0 ? normals[vnIdx] : Vector3.UnitZ;

                        meshVertices.Add(new Vertex(position, color, texCoord, normal));
                        indices[i] = meshVertices.Count - 1;
                    }
                    tris.Add((indices[2], indices[1], indices[0])); // reversed winding order
                }
            }

            var mesh = new Mesh();
            mesh.Vertices.AddRange(meshVertices);
            mesh.Tris.AddRange(tris);
            return mesh;
        }
    }
}
