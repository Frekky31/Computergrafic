using Rasterization.Objects;
using Raylib_cs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Rasterization.Core
{
    public class Engine
    {


        public static void Run(int width, int height)
        {
            Raylib.SetConfigFlags(ConfigFlags.VSyncHint | ConfigFlags.ResizableWindow);
            Raylib.InitWindow(width, height, "Rasterization");

            var orbitCenter = new Vector3(0, 0, 4);
            Objects.Mesh cube = Objects.Mesh.CreateCube(new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 1, 0), new(1, 0, 1), new(0, 1, 1));
            //var cat = MeshLoader.LoadObj("Meshes/cat.obj", new Vector3(0.97f, 0.002f, 0.298f));

            while (!Raylib.WindowShouldClose())
            {
                float time = (float)Raylib.GetTime();
                Matrix4x4 cubeRotation =
                    Matrix4x4.CreateRotationY(time / 2f) *
                    Matrix4x4.CreateRotationX(time) *
                    Matrix4x4.CreateRotationZ(time * 1.4f);

                Matrix4x4 moveToOrbit = Matrix4x4.CreateTranslation(orbitCenter);

                Matrix4x4 orbitRotation = Matrix4x4.CreateRotationY(time);
                Matrix4x4 orbitRotationZ = Matrix4x4.CreateRotationZ(MathF.PI / 8);

                Matrix4x4 modelMatrix = cubeRotation * moveToOrbit * orbitRotation;

                Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(new Vector3(0, 0, -7), new Vector3(0, 0, 0), -Vector3.UnitY);
                Matrix4x4 projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2, 1, 0.1f, 100f);

                Matrix4x4 mvp = modelMatrix * viewMatrix * projectionMatrix;

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                foreach (var (A, B, C) in cube.Tris)
                {
                    var v1 = VertexShader(cube.Vertices[A], mvp);
                    var v2 = VertexShader(cube.Vertices[B], mvp);
                    var v3 = VertexShader(cube.Vertices[C], mvp);

                    v1 *= (1f / v1.Position.W);
                    v2 *= (1f / v2.Position.W);
                    v3 *= (1f / v3.Position.W);

                    var halfWidth = (float)Raylib.GetScreenWidth() / 2;
                    var halfHeight = (float)Raylib.GetScreenHeight() / 2;

                    var p1 = new Vector2(v1.Position.X * halfWidth + halfWidth, v1.Position.Y * halfWidth + halfHeight);
                    var p2 = new Vector2(v2.Position.X * halfWidth + halfWidth, v2.Position.Y * halfWidth + halfHeight);
                    var p3 = new Vector2(v3.Position.X * halfWidth + halfWidth, v3.Position.Y * halfWidth + halfHeight);

                    var edge1 = p2 - p1;
                    var edge2 = p3 - p1;
                    float cross = edge1.X * edge2.Y - edge1.Y * edge2.X;
                    if (cross >= 0) continue;

                    Raylib.DrawTriangleLines(p1, p2, p3, Color.Black);
                    var color = new Color(FloatToSrgbByte(v1.Color.X), FloatToSrgbByte(v1.Color.Y), FloatToSrgbByte(v1.Color.Z));
                    Raylib.DrawTriangle(p1, p2, p3, color);
                }

                Raylib.DrawText($"FPS: {Raylib.GetFPS()}", 10, 10, 21, Color.White);

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }

        public static Vertex VertexShader(Vertex v, Matrix4x4 mvp)
        {
            Vector4 transformed = Vector4.Transform(v.Position, mvp);
            return new Vertex(transformed, v.WorldCoordinates, v.Color, v.TexCoord, v.Normal);
        }

        private static byte FloatToSrgbByte(float c)
        {
            c = MathF.Pow(c, 1.0f / 2.2f);
            c = Math.Clamp(c, 0f, 1f);
            return (byte)(c * 255f);
        }
    }
}
