using OpenGL.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.IO;

namespace OpenGL.Scenes
{
    public class ManyObjectsScene : Scene
    {
        // store per-cube data so UpdateScene can animate each cube
        private readonly List<Vector3> _cubePositions = new();
        private readonly List<float> _cubeSpeeds = new();
        private readonly List<string> _cubeNames = new();

        public override void LoadScene(int hProgram)
        {
            // Materials
            var matBricks = new Material()
            {
                Color = new Vector3(1, 0.3f, 1),
                Shininess = 32f,
                SpecularStrength = 0.3f,
                HasTextureAlpha = false
            };
            var matCloud = new Material()
            {
                Color = new Vector3(1f, 1f, 1f),
                Shininess = 32f,
                SpecularStrength = 0.3f,
                HasTextureAlpha = true
            };

            var matFire = new Material()
            {
                Color = new Vector3(1f, 1f, 1f),
                Shininess = 32f,
                SpecularStrength = 0.3f,
                HasTextureAlpha = true
            };

            // Load textures (Program.LoadTexture generates mipmaps and preserves alpha)
            var pathBricks = Path.GetFullPath("Textures/bricks.jpg");
            matBricks.TextureHandle = Program.LoadTexture(pathBricks);

            var pathCloud = Path.GetFullPath("Textures/cloud.png");
            matCloud.TextureHandle = Program.LoadTexture(pathCloud);

            var pathFire = Path.GetFullPath("Textures/fire.png");
            matFire.TextureHandle = Program.LoadTexture(pathFire);

            var BgCube = Mesh.CreateCube(new(1, 1, 1), new(1, 1, 1), new(1, 1, 1),
                                  new(1, 1, 1), new(1, 1, 1), new(1, 1, 1));
            var bgNode = new SceneGraphNode(BgCube.Vertices, BgCube.Tris, "bgCube");
            bgNode.Material = matBricks;

            Root.AddChild(bgNode, Matrix4x4.CreateScale(50f) * Matrix4x4.CreateTranslation(new Vector3(0, 0, -30f)));

            var fgCube = Mesh.CreateCube(new(1, 1, 1), new(1, 1, 1), new(1, 1, 1),
                                  new(1, 1, 1), new(1, 1, 1), new(1, 1, 1));
            var fgNode = new SceneGraphNode(fgCube.Vertices, fgCube.Tris, "fgCube");
            fgNode.Material = matFire;

            Root.AddChild(fgNode, Matrix4x4.CreateScale(50f) * Matrix4x4.CreateTranslation(new Vector3(0, 0, -30f)));

            // Create 16 cubes in a 4x4 grid, background z
            int grid = 4;
            float spacing = 3.0f;                 // horizontal and vertical spacing
            float centerOffsetX = (grid - 1) * spacing * 0.5f;
            float centerOffsetY = (grid - 1) * spacing * 0.5f;
            float backgroundZ = -20f;
            float cubeScale = 1.2f;

            for (int row = 0; row < grid; row++)
            {
                for (int col = 0; col < grid; col++)
                {
                    int idx = row * grid + col;
                    var mesh = Mesh.CreateCube(new(1, 0, 0), new(0, 1, 0), new(0, 0, 1),
                                               new(1, 1, 0), new(1, 0, 1), new(0, 1, 1));
                    string name = $"cube{idx}";
                    var node = new SceneGraphNode(mesh.Vertices, mesh.Tris, name);

                    node.Material = (idx % 2 == 0) ? matCloud : matBricks;

                    float x = col * spacing - centerOffsetX;
                    float y = row * spacing - centerOffsetY;
                    float z = backgroundZ;

                    _cubePositions.Add(new Vector3(x, y, z));
                    _cubeSpeeds.Add(0.5f + (idx % 5) * 0.15f);
                    _cubeNames.Add(name);

                    var initial = Matrix4x4.CreateTranslation(new Vector3(x, y, z)) * Matrix4x4.CreateRotationY(0f) * Matrix4x4.CreateScale(cubeScale);
                    Root.AddChild(node, initial);
                }
            }

            // lights
            SceneGraphNode.ClearGlobalLights();
            SceneGraphNode.AddGlobalLight(new LightObject(new Vector3(1f, 1f, 1f), new Vector3(5, -7, 4), 1.0f));

            Root.Load(hProgram);
        }

        public override void RenderScene(int hProgram, float time, GameWindow w)
        {
            var matrix = Matrix4x4.Identity;
            var v = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4, (float)w.Size.X / w.Size.Y, 0.1f, 100f);

            GL.Uniform1(GL.GetUniformLocation(hProgram, "inTime"), (float)time);
            Root.Render(matrix, v, hProgram, (float)time);
        }

        public override void UpdateScene(int hProgram, float time)
        {
            float cubeScale = 1.2f;

            for (int i = 0; i < _cubeNames.Count; i++)
            {
                var pos = _cubePositions[i];
                float speed = _cubeSpeeds[i];

                var rot = Matrix4x4.CreateRotationY((float)time * speed) * Matrix4x4.CreateRotationX((float)time * (speed * 0.6f));
                var scale = Matrix4x4.CreateScale(cubeScale);
                var translation = Matrix4x4.CreateTranslation(pos);

                var final = rot * translation * scale;

                Root.SetChildTransform(_cubeNames[i], final);
            }

            Root.SetChildTransform("bgCube",
                Matrix4x4.CreateRotationY((float)time * 0.1f) *
                Matrix4x4.CreateRotationX((float)time * 0.05f) *
                Matrix4x4.CreateScale(4f) *
                Matrix4x4.CreateTranslation(new Vector3(0, 0, -30f))
            );

            Root.SetChildTransform("fgCube",
                Matrix4x4.CreateRotationY((float)time * 0.1f) *
                Matrix4x4.CreateRotationX((float)time * 0.05f) *
                Matrix4x4.CreateScale(4f) *
                Matrix4x4.CreateTranslation(new Vector3(0, 0, -10f))
            );
        }
    }
}
