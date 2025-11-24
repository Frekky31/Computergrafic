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

            SceneGraphNode cube1Node = new(cube1.Vertices, cube1.Tris, "cube1");
            SceneGraphNode cube2Node = new(cube2.Vertices, cube2.Tris, "cube2");
            SceneGraphNode sphere1Node = new(sphere1.Vertices, sphere1.Tris, "sphere1");
            Root.AddChild(cube1Node, Matrix4x4.Identity);
            Root.AddChild(cube2Node, Matrix4x4.Identity);
            Root.AddChild(sphere1Node, Matrix4x4.Identity);

            Root.Load(hProgram);
        }

        public override void RenderScene(int hProgram, float time, GameWindow w)
        {
            var matrix = Matrix4x4.Identity;
            matrix *= Matrix4x4.CreateRotationY((float)time * 0.5f);
            matrix *= Matrix4x4.CreateRotationX((float)time * 0.3f);
            matrix *= Matrix4x4.CreateTranslation(0, 0, -10);

            var v = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4, (float)w.Size.X / w.Size.Y, 0.1f, 100f);

            GL.Uniform1(GL.GetUniformLocation(hProgram, "inTime"), (float)time);
            Root.Render(matrix, v, hProgram, (float)time);
        }

        public override void UpdateScene(int hProgram, float time)
        {

            Root.SetChildTransform("cube1",
                Matrix4x4.CreateRotationY((float)time * 0.5f) *
                Matrix4x4.CreateRotationX((float)time * 0.3f) *
                Matrix4x4.CreateRotationZ((float)time * 0.2f) *
                Matrix4x4.CreateTranslation(3, 0, 0));

            Root.SetChildTransform("cube2",
                Matrix4x4.CreateRotationY((float)time * 0.5f) *
                Matrix4x4.CreateRotationX((float)time * 0.3f) *
                Matrix4x4.CreateRotationZ((float)time * 0.2f) *
                Matrix4x4.CreateTranslation(-3, 0, 0));
        }
    }
}
