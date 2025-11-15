using Rasterization.Objects;
using Raylib_cs;
using System;
using System.IO;
using System.Numerics;
using Color = Raylib_cs.Color;

namespace Rasterization.Core
{
    public class Engine
    {
        public static float zNear = 0.1f;
        public static float zFar = 100f;

        static LightObject light = new() { Color = new Vector3(3, 3, 3), Position = new Vector3(5, 8, -4) };
        static Vector3 E = new(0, 0, -7);
        static Vector3 PSpecular = Vector3.One;
        static int specularK = 50;

        public static void Run(int width, int height)
        {
            Raylib.InitWindow(width, height, "Rasterizer (Scene Graph - Textured)");

            SceneGraphNode.Width = width;
            SceneGraphNode.Height = height;
            SceneGraphNode.HalfWidth = width / 2f;
            SceneGraphNode.HalfHeight = height / 2f;
            SceneGraphNode.PixelBuffer = new Color[width, height];
            SceneGraphNode.ZBuffer = new float[width, height];

            var root = new SceneGraphNode();

            var cube1Mesh = Objects.Mesh.CreateCube(new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 1, 0), new(1, 0, 1), new(0, 1, 1));
            var cube2Mesh = Objects.Mesh.CreateCube(new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 1, 0), new(1, 0, 1), new(0, 1, 1));
            var sphereMesh = Objects.Mesh.CreateSphere(5, new Vector3(0.8f, 0.01f, 0.1f));

            var cube1 = new SceneGraphNode(cube1Mesh.Vertices, cube1Mesh.Tris);
            var cube2 = new SceneGraphNode(cube2Mesh.Vertices, cube2Mesh.Tris);
            var sphere = new SceneGraphNode(sphereMesh.Vertices, sphereMesh.Tris);

            root.AddChild(cube1, Matrix4x4.Identity);
            root.AddChild(cube2, Matrix4x4.Identity);
            root.AddChild(sphere, Matrix4x4.Identity);

            try
            {
                var bricksPath = Path.Combine(AppContext.BaseDirectory, "Textures", "bricks.jpg");
                if (File.Exists(bricksPath))
                {
                    var tex = Texture.FromBitmapFile(bricksPath);
                    cube1.Texture = tex;
                    cube2.Texture = tex;
                }
            }
            catch
            {
            }

            while (!Raylib.WindowShouldClose())
            {
                float t = (float)Raylib.GetTime();

                // clear Z-buffer & pixel buffer
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        SceneGraphNode.PixelBuffer[x, y] = Color.Black;
                        SceneGraphNode.ZBuffer[x, y] = float.PositiveInfinity;
                    }

                var view = Matrix4x4.CreateLookAt(E, Vector3.Zero, -Vector3.UnitY);
                var proj = Matrix4x4.CreatePerspectiveFieldOfView(
                    MathF.PI / 2, width / (float)height, zNear, zFar);
                var vp = view * proj;

                var frustum = Frustum.FromMatrix(vp);

                cube1.Render(
                    Matrix4x4.CreateRotationY(t) *
                    Matrix4x4.CreateTranslation(new Vector3(-2, 0, 4)),
                    vp, frustum);

                cube2.Render(
                    Matrix4x4.CreateRotationX(t * 2) *
                    Matrix4x4.CreateTranslation(new Vector3(2, 0, 4)),
                    vp, frustum);

                sphere.Render(
                    Matrix4x4.CreateRotationY(-t) *
                    Matrix4x4.CreateTranslation(new Vector3(0, 0, 4)),
                    vp, frustum);

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        Raylib.DrawPixel(x, y, SceneGraphNode.PixelBuffer[x, y]);

                Raylib.DrawText($"FPS: {Raylib.GetFPS()}", 10, 10, 20, Color.White);
                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }

        public static Vector3 FragmentShader(Vertex v)
        {
            Vector3 PL = Vector3.Normalize(light.Position - v.WorldCoordinates);
            Vector3 n = Vector3.Normalize(v.Normal);
            float ndotl = MathF.Max(0, Vector3.Dot(n, PL));

            Vector3 ambient = 0.05f * v.Color;
            Vector3 diffuse = v.Color * light.Color * ndotl;

            Vector3 spec = Vector3.Zero;
            if (ndotl > 0)
            {
                Vector3 viewDir = Vector3.Normalize(E - v.WorldCoordinates);
                Vector3 half = Vector3.Normalize(viewDir + PL);
                float s = MathF.Max(0, Vector3.Dot(n, half));
                spec = light.Color * PSpecular * MathF.Pow(s, specularK);
            }

            return ambient + diffuse + spec;
        }

        public static (float, float) Rasterization(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            Vector2 ab = b - a;
            Vector2 ac = c - a;
            Vector2 ap = p - a;
            float inv = 1f / (ab.X * ac.Y - ac.X * ab.Y);
            float u = (ap.X * ac.Y - ac.X * ap.Y) * inv;
            float v = (ab.X * ap.Y - ap.X * ab.Y) * inv;
            return (u, v);
        }

        public static Color Vector2Color(Vector3 c)
        {
            byte r = FloatToSrgbByte(c.X);
            byte g = FloatToSrgbByte(c.Y);
            byte b = FloatToSrgbByte(c.Z);
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
