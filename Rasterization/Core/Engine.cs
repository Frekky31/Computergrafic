using Rasterization.Objects;
using Raylib_cs;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using Color = Raylib_cs.Color;

namespace Rasterization.Core
{
    public class Engine
    {

        static LightObject light = new() { Color = new Vector3(3, 3, 3), Position = new Vector3(5, 8, -4) };
        protected static Vector3 E = new Vector3(0, 0, -7);
        protected static Vector3 PSpecular = Vector3.One;
        protected static int specularK = 50;
        private static Objects.Mesh[] meshes = [];

        public static void Run(int width, int height)
        {
            Raylib.SetConfigFlags(ConfigFlags.VSyncHint | ConfigFlags.ResizableWindow);
            Raylib.InitWindow(width, height, "Rasterization");

            var orbitCenter = new Vector3(0, 0, 4);
            meshes =
            [
                Objects.Mesh.CreateCube(new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 1, 0), new(1, 0, 1), new(0, 1, 1)),
                Objects.Mesh.CreateCube(new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 1, 0), new(1, 0, 1), new(0, 1, 1)),
                Objects.Mesh.CreateSphere(5, new Vector3(0.8f, 0.01f, 0.1f))
            ];
            float zNear = 0.1f;
            float zFar = 100f;

            var halfWidth = (float)Raylib.GetScreenWidth() / 2;
            var halfHeight = (float)Raylib.GetScreenHeight() / 2;


            while (!Raylib.WindowShouldClose())
            {
                float time = (float)Raylib.GetTime();
                Matrix4x4 cubeRotation =
                    Matrix4x4.CreateRotationZ(time * -2f) *
                    Matrix4x4.CreateRotationY(time) *
                    Matrix4x4.CreateRotationX(time * 0.5f);


                Matrix4x4 cubeRotation2 =
                    Matrix4x4.CreateRotationZ(time) *
                    Matrix4x4.CreateRotationY(time * 1.3f) *
                    Matrix4x4.CreateRotationX(time * 2f);

                Matrix4x4 moveToOrbit = Matrix4x4.CreateTranslation(orbitCenter);

                Matrix4x4 orbitRotation = Matrix4x4.CreateRotationY(time);
                Matrix4x4 orbitRotation2 = Matrix4x4.CreateRotationY(-time);
                Matrix4x4 orbitRotationZ = Matrix4x4.CreateRotationZ(MathF.PI / 8);

                Matrix4x4 modelMatrix = cubeRotation;

                Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(E, new Vector3(0, 0, 0), -Vector3.UnitY);
                Matrix4x4 projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2, 1, zNear, zFar);

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                Color[,] pixelBuffer = new Color[width, height];
                float[,] zBuffer = new float[width, height];
                for (int yy = 0; yy < height; yy++)
                    for (int xx = 0; xx < width; xx++)
                        zBuffer[xx, yy] = float.PositiveInfinity;

                foreach (var mesh in meshes)
                {
                    if(mesh == meshes[1])
                        modelMatrix = cubeRotation2* moveToOrbit * orbitRotation;
                    else if(mesh == meshes[2])
                        modelMatrix = moveToOrbit * orbitRotation2;
                    else
                        modelMatrix = cubeRotation * moveToOrbit * orbitRotation;
                    foreach (var (A, B, C) in mesh.Tris)
                    {
                        var v1 = VertexShader(mesh.Vertices[A], modelMatrix, viewMatrix, projectionMatrix);
                        var v2 = VertexShader(mesh.Vertices[B], modelMatrix, viewMatrix, projectionMatrix);
                        var v3 = VertexShader(mesh.Vertices[C], modelMatrix, viewMatrix, projectionMatrix);

                        v1 *= (1f / v1.Position.W);
                        v2 *= (1f / v2.Position.W);
                        v3 *= (1f / v3.Position.W);


                        var p1 = new Vector2(v1.Position.X * halfWidth + halfWidth, v1.Position.Y * halfHeight + halfHeight);
                        var p2 = new Vector2(v2.Position.X * halfWidth + halfWidth, v2.Position.Y * halfHeight + halfHeight);
                        var p3 = new Vector2(v3.Position.X * halfWidth + halfWidth, v3.Position.Y * halfHeight + halfHeight);

                        var edge1 = p2 - p1;
                        var edge2 = p3 - p1;
                        float cross = edge1.X * edge2.Y - edge1.Y * edge2.X;
                        if (cross >= 0) continue;

                        int minX = (int)MathF.Max(0, MathF.Floor(MathF.Min(p1.X, MathF.Min(p2.X, p3.X))));
                        int maxX = (int)MathF.Min(width - 1, MathF.Ceiling(MathF.Max(p1.X, MathF.Max(p2.X, p3.X))));
                        int minY = (int)MathF.Max(0, MathF.Floor(MathF.Min(p1.Y, MathF.Min(p2.Y, p3.Y))));
                        int maxY = (int)MathF.Min(height - 1, MathF.Ceiling(MathF.Max(p1.Y, MathF.Max(p2.Y, p3.Y))));


                        Parallel.ForEach(Partitioner.Create(minY, maxY), (range, state) =>
                        {
                            for (int y = range.Item1; y < range.Item2; y++)
                            {
                                for (int x = minX; x < maxX; x++)
                                {
                                    (float u, float v) = Rasterization(p1, p2, p3, new Vector2(x, y));
                                    if (u >= 0 && v >= 0 && (u + v) < 1)
                                    {
                                        var q = v1 + u * (v2 - v1) + v * (v3 - v1);
                                        var z = zFar * zNear / (zFar + (zNear - zFar) * q.Position.Z);
                                        var q1 = z * q;
                                        if (z < zBuffer[x, y])
                                        {
                                            zBuffer[x, y] = z;

                                            Vector3 colorVec = FragmentShader(q1);
                                            pixelBuffer[x, y] = Vector2Color(colorVec);
                                        }
                                    }
                                }
                            }
                        });
                    }

                }

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (pixelBuffer[x, y].A != 0)
                            Raylib.DrawPixel(x, y, pixelBuffer[x, y]);
                    }
                }

                Raylib.DrawText($"FPS: {Raylib.GetFPS()}", 10, 10, 21, Raylib_cs.Color.White);

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }

        public static (float, float) Rasterization(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 p)
        {
            Vector2 ab = new(v2.X - v1.X, v2.Y - v1.Y);
            Vector2 ac = new(v3.X - v1.X, v3.Y - v1.Y);
            Vector2 ap = new(p.X - v1.X, p.Y - v1.Y);
            var mult = 1.0f / (ab.X * ac.Y - ac.X * ab.Y);
            float u = (ap.X * ac.Y - ac.X * ap.Y) * mult;
            float v = (ab.X * ap.Y - ap.X * ab.Y) * mult;
            return (u, v);
        }

        public static Vertex VertexShader(Vertex v, Matrix4x4 model, Matrix4x4 view, Matrix4x4 projection)
        {
            Vector4 pos = v.Position;

            _ = Matrix4x4.Invert(model, out Matrix4x4 invModel);

            Vector4 newPosition = Vector4.Transform(v.Position, model * view * projection);
            Vector4 newWorldPos = Vector4.Transform(v.Position, Matrix4x4.Multiply(Matrix4x4.Transpose(invModel), model.GetDeterminant()));
            Vector3 newNormal = Vector3.Normalize(Vector3.Transform(v.Normal, model));
            return new Vertex(newPosition, new(newWorldPos.X, newWorldPos.Y, newWorldPos.Z), v.Color, v.TexCoord, newNormal);
        }

        public static Vector3 FragmentShader(Vertex v)
        {
            Vector3 PL = Vector3.Normalize(light.Position - v.WorldCoordinates);

            Vector3 n = Vector3.Normalize(v.Normal);

            float nDotL = MathF.Max(0f, Vector3.Dot(n, PL));

            Vector3 ambient = 0.05f * v.Color;
            Vector3 diffuse = v.Color * light.Color * nDotL;

            Vector3 specular = Vector3.Zero;
            if (nDotL > 0f)
            {
                Vector3 viewDir = Vector3.Normalize(E - v.WorldCoordinates);
                Vector3 half = Vector3.Normalize(viewDir + PL);
                float specAngle = MathF.Max(0f, Vector3.Dot(n, half));
                specular = light.Color * PSpecular * MathF.Pow(specAngle, specularK);
            }

            Vector3 color = ambient + diffuse + specular;

            color.X = MathF.Max(0f, color.X);
            color.Y = MathF.Max(0f, color.Y);
            color.Z = MathF.Max(0f, color.Z);

            return color;
        }

        private static Color Vector2Color(Vector3 color)
        {
            return new Color(FloatToSrgbByte(color.X), FloatToSrgbByte(color.Y), FloatToSrgbByte(color.Z));
        }

        private static byte FloatToSrgbByte(float c)
        {
            c = MathF.Pow(c, 1.0f / 2.2f);
            c = Math.Clamp(c, 0f, 1f);
            return (byte)(c * 255f);
        }
    }
}
