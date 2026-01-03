using OpenGL.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.Scenes
{
    public class MainScene : Scene
    {


        public override void LoadScene(int hProgram)
        {
            var cube1 = Mesh.CreateCube(new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 1, 0), new(1, 0, 1), new(0, 1, 1));
            var cube2 = Mesh.CreateCube(new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 1, 0), new(1, 0, 1), new(0, 1, 1));
            var sphere1 = Mesh.CreateSphere(16, new(1, 1, 1));

            Material matBricks = new()
            {
                Color = new Vector3(1, 0.3f, 1),
                Shininess = 32f,
                SpecularStrength = 0.3f
            };

            var path = Path.GetFullPath("Textures/bricks.jpg");
            var hTexture = Program.LoadTexture(path);
            matBricks.TextureHandle = hTexture;

            SceneGraphNode cube1Node = new(cube1.Vertices, cube1.Tris, "cube1");
            SceneGraphNode cube2Node = new(cube2.Vertices, cube2.Tris, "cube2");
            SceneGraphNode sphere1Node = new(sphere1.Vertices, sphere1.Tris, "sphere1");
            cube1Node.Material = matBricks;
            cube2Node.Material = matBricks;
            sphere1Node.Material = matBricks;
            Root.AddChild(cube1Node, Matrix4x4.Identity);
            Root.AddChild(cube2Node, Matrix4x4.Identity);
            Root.AddChild(sphere1Node, Matrix4x4.Identity);

            // after shader program is linked and before rendering
            SceneGraphNode.ClearGlobalLights();
            cube1Node.Lights.Add(new LightObject(new Vector3(1, 0.4f, 0.3f), new Vector3(5, 8, -4), 2.0f));
            SceneGraphNode.AddGlobalLight(new LightObject(new Vector3(1, 1f, 1f), new Vector3(5, -7, 4), 1.0f));

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
            var matrix = Matrix4x4.Identity;
            matrix *= Matrix4x4.CreateRotationY((float)time * 0.5f);
            matrix *= Matrix4x4.CreateRotationX((float)time * 0.3f);
            matrix *= Matrix4x4.CreateTranslation(0, 0, -10);

            Root.SetChildTransform("sphere1", matrix);

            Root.SetChildTransform("cube1",
                Matrix4x4.CreateRotationY((float)time * 2.5f) *
                Matrix4x4.CreateRotationX((float)time * 0.3f) *
                Matrix4x4.CreateRotationZ((float)time * 0.2f) *
                Matrix4x4.CreateTranslation(3, 0, 0) * matrix);

            Root.SetChildTransform("cube2",
                Matrix4x4.CreateRotationY((float)time * 0.5f) *
                Matrix4x4.CreateRotationX((float)time * 0.3f) *
                Matrix4x4.CreateRotationZ((float)time * 0.2f) *
                Matrix4x4.CreateTranslation(-3, 0, 0) * matrix);
        }
    }
}
