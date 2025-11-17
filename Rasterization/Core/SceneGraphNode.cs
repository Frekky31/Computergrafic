using Rasterization.Objects;
using Raylib_cs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Rasterization.Core
{

    public class SceneGraphNode
    {
        public static Raylib_cs.Color[,] PixelBuffer = null!;
        public static float[,] ZBuffer = null!;
        public static int Width;
        public static int Height;
        public static float HalfWidth;
        public static float HalfHeight;

        public Texture? Texture { get; set; }

        public Vertex[] Vertices { get; private set; } = Array.Empty<Vertex>();
        public (int A, int B, int C)[] Tris { get; private set; } = Array.Empty<(int, int, int)>();
        public List<(SceneGraphNode Node, Matrix4x4 Transform)> Children { get; } = new();
        public Volume BoundingVolume { get; private set; } = new();

        public Action<Vertex, Vertex, Vertex>? OnTriangle;

        protected static LightObject light = new() { Color = new Vector3(3, 3, 3), Position = new Vector3(5, 8, -4) };
        protected static Vector3 E = new Vector3(0, 0, -7);
        protected static Vector3 PSpecular = Vector3.One;
        protected static int specularK = 50;

        public SceneGraphNode() { }

        public SceneGraphNode(List<Vertex> verts, List<(int A, int B, int C)> tris)
        {
            Vertices = verts.ToArray();
            Tris = tris.ToArray();
            ComputeBoundingVolume();
        }

        public void SetGeometry(Vertex[] verts, (int A, int B, int C)[] tris)
        {
            Vertices = verts;
            Tris = tris;
            ComputeBoundingVolume();
        }

        public void AddChild(SceneGraphNode child, Matrix4x4 local)
            => Children.Add((child, local));

        private void ComputeBoundingVolume()
        {
            if (Vertices == null || Vertices.Length == 0)
            {
                BoundingVolume = new Volume();
                return;
            }

            Vector3 center = Vector3.Zero;
            foreach (var v in Vertices) center += v.WorldCoordinates;
            center /= Vertices.Length;

            float r = 0f;
            foreach (var v in Vertices)
            {
                float d = Vector3.Distance(center, v.WorldCoordinates);
                if (d > r) r = d;
            }
            BoundingVolume = new Volume(new BoundingSphere(center, r));
        }

        public void Render(Matrix4x4 model, Matrix4x4 viewProj, Frustum frustum)
        {
            if (BoundingVolume.HasSphere)
            {
                var sphere = BoundingVolume.Sphere!.Transform(model);
                if (!frustum.ContainsSphere(sphere.Center, sphere.Radius))
                    return;
            }

            Matrix4x4 normalM;
            if (Matrix4x4.Invert(model, out var inv))
                normalM = Matrix4x4.Transpose(inv);
            else
                normalM = Matrix4x4.Transpose(model);

            RenderTriangles(model, viewProj, normalM);

            foreach (var c in Children)
            {
                var childModel = c.Transform * model;
                c.Node.Render(childModel, viewProj, frustum);
            }
        }

        private void RenderTriangles(Matrix4x4 model, Matrix4x4 viewProj, Matrix4x4 normalM)
        {
            if (Vertices == null || Vertices.Length == 0 || Tris == null || Tris.Length == 0) return;

            Matrix4x4 mvp = model * viewProj;

            for (int i = 0; i < Tris.Length; i++)
            {
                var (A, B, C) = Tris[i];

                var vClip1 = Transform(Vertices[A], model, mvp, normalM);
                var vClip2 = Transform(Vertices[B], model, mvp, normalM);
                var vClip3 = Transform(Vertices[C], model, mvp, normalM);

                OnTriangle?.Invoke(vClip1, vClip2, vClip3);

                float invW1 = 1f / vClip1.Position.W;
                float invW2 = 1f / vClip2.Position.W;
                float invW3 = 1f / vClip3.Position.W;

                var ndc1 = vClip1.Position * invW1;
                var ndc2 = vClip2.Position * invW2;
                var ndc3 = vClip3.Position * invW3;

                var p1 = new Vector2(ndc1.X * HalfWidth + HalfWidth, ndc1.Y * HalfHeight + HalfHeight);
                var p2 = new Vector2(ndc2.X * HalfWidth + HalfWidth, ndc2.Y * HalfHeight + HalfHeight);
                var p3 = new Vector2(ndc3.X * HalfWidth + HalfWidth, ndc3.Y * HalfHeight + HalfHeight);

                var e1 = p2 - p1;
                var e2 = p3 - p1;
                float cross = e1.X * e2.Y - e1.Y * e2.X;
                if (cross >= 0) continue;

                int minX = (int)MathF.Max(0, MathF.Floor(MathF.Min(p1.X, MathF.Min(p2.X, p3.X))));
                int maxX = (int)MathF.Min(Width - 1, MathF.Ceiling(MathF.Max(p1.X, MathF.Max(p2.X, p3.X))));
                int minY = (int)MathF.Max(0, MathF.Floor(MathF.Min(p1.Y, MathF.Min(p2.Y, p3.Y))));
                int maxY = (int)MathF.Min(Height - 1, MathF.Ceiling(MathF.Max(p1.Y, MathF.Max(p2.Y, p3.Y))));

                int maxYExclusive = maxY + 1;
                int maxXExclusive = maxX + 1;

                Parallel.ForEach(Partitioner.Create(minY, maxYExclusive), (range) =>
                {
                    for (int y = range.Item1; y < range.Item2; y++)
                    {
                        for (int x = minX; x < maxXExclusive; x++)
                        {
                            (float u, float v) = Engine.Rasterization(p1, p2, p3, new Vector2(x + 0.5f, y + 0.5f));
                            if (u >= 0 && v >= 0 && (u + v) < 1)
                            {
                                float w0 = 1f - u - v;
                                float w1 = u;
                                float w2 = v;

                                float interpInvW = invW1 * w0 + invW2 * w1 + invW3 * w2;
                                if (interpInvW <= 0) continue;

                                float ndcZ_numer = vClip1.Position.Z * invW1 * w0 + vClip2.Position.Z * invW2 * w1 + vClip3.Position.Z * invW3 * w2;
                                float ndcZ = ndcZ_numer / interpInvW;

                                float z = Engine.zFar * Engine.zNear / (Engine.zFar + (Engine.zNear - Engine.zFar) * ndcZ);

                                if (z < ZBuffer[x, y])
                                {
                                    ZBuffer[x, y] = z;

                                    Vector3 worldNumer =
                                        vClip1.WorldCoordinates * invW1 * w0 +
                                        vClip2.WorldCoordinates * invW2 * w1 +
                                        vClip3.WorldCoordinates * invW3 * w2;
                                    Vector3 worldInterp = worldNumer / interpInvW;

                                    Vector3 colorNumer =
                                        vClip1.Color * invW1 * w0 +
                                        vClip2.Color * invW2 * w1 +
                                        vClip3.Color * invW3 * w2;
                                    Vector3 colorInterp = colorNumer / interpInvW;

                                    Vector2 texNumer =
                                        vClip1.TexCoord * invW1 * w0 +
                                        vClip2.TexCoord * invW2 * w1 +
                                        vClip3.TexCoord * invW3 * w2;
                                    Vector2 texInterp = texNumer / interpInvW;

                                    Vector3 normalNumer =
                                        vClip1.Normal * invW1 * w0 +
                                        vClip2.Normal * invW2 * w1 +
                                        vClip3.Normal * invW3 * w2;
                                    Vector3 normalInterp = Vector3.Normalize(normalNumer / interpInvW);

                                    var fragVertex = new Vertex(new Vector4(0, 0, ndcZ, 1f), worldInterp, colorInterp, texInterp, normalInterp);

                                    Vector3 colorVec;

                                    if (this.Texture != null)
                                    {
                                        Vector3 texColorLinear = this.Texture.SampleBilinear(texInterp);
                                        colorVec = FragmentShaderWithBaseColor(fragVertex, texColorLinear);
                                    }
                                    else
                                    {
                                        colorVec = Engine.FragmentShader(fragVertex);
                                    }

                                    PixelBuffer[x, y] = Vector2Color(colorVec);
                                }
                            }
                        }
                    }
                });
            }
        }

        private static Vertex Transform(Vertex v, Matrix4x4 model, Matrix4x4 mvp, Matrix4x4 normalM)
        {
            Vector4 posClip = Vector4.Transform(v.Position, mvp);
            Vector4 posWorld4 = Vector4.Transform(v.Position, model);
            Vector3 world = new(posWorld4.X, posWorld4.Y, posWorld4.Z);
            Vector3 normal = Vector3.Normalize(Vector3.Transform(v.Normal, normalM));
            return new Vertex(posClip, world, v.Color, v.TexCoord, normal);
        }

        private Vector3 FragmentShaderWithBaseColor(Vertex v, Vector3 baseColorLinear)
        {
            Vector3 PL = Vector3.Normalize(light.Position - v.WorldCoordinates);
            Vector3 n = Vector3.Normalize(v.Normal);

            float nDotL = MathF.Max(0f, Vector3.Dot(n, PL));

            Vector3 ambient = 0.15f * baseColorLinear;

            Vector3 diffuse = baseColorLinear * nDotL * light.Color;

            Vector3 specular = Vector3.Zero;
            if (nDotL > 0f)
            {
                Vector3 viewDir = Vector3.Normalize(E - v.WorldCoordinates);
                Vector3 half = Vector3.Normalize(viewDir + PL);
                float specAngle = MathF.Max(0f, Vector3.Dot(n, half));
                specular = light.Color * PSpecular * MathF.Pow(specAngle, specularK);
            }

            Vector3 col = ambient + diffuse + specular;

            // Clamp
            col.X = MathF.Max(0f, col.X);
            col.Y = MathF.Max(0f, col.Y);
            col.Z = MathF.Max(0f, col.Z);

            return col;
        }


        private static Color Vector2Color(Vector3 colorLinear)
        {
            byte r = FloatToSrgbByte(colorLinear.X);
            byte g = FloatToSrgbByte(colorLinear.Y);
            byte b = FloatToSrgbByte(colorLinear.Z);
            return new Color(r, g, b);
        }

        private static byte FloatToSrgbByte(float c)
        {
            c = Math.Clamp(c, 0f, 1f);
            c = MathF.Pow(c, 1f / 2.2f);
            return (byte)(c * 255f);
        }
    }
}
