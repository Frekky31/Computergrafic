using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenGL.Objects;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using var w = new GameWindow(
                new GameWindowSettings() { },
                new NativeWindowSettings()
                {
                    API = ContextAPI.OpenGL,
                    Flags = ContextFlags.ForwardCompatible | ContextFlags.Debug,
                    SrgbCapable = true,
                    ClientSize = new OpenTK.Mathematics.Vector2i(720, 480),
                    Title = "ComGr",
                    APIVersion = new Version(4, 1),
                });

            int hProgram = 0;
            int tiuIndex = 1;

            var cube1 = Mesh.CreateCube(new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 1, 0), new(1, 0, 1), new(0, 1, 1));
            var cube2 = Mesh.CreateCube(new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 1, 0), new(1, 0, 1), new(0, 1, 1));

            SceneGraphNode root = new();
            SceneGraphNode cube1Node = new(cube1.Vertices, cube1.Tris);
            SceneGraphNode cube2Node = new(cube2.Vertices, cube2.Tris);
            root.AddChild(cube1Node, Matrix4x4.Identity);
            root.AddChild(cube2Node, Matrix4x4.Identity);

            w.Load += () =>
            {
                //set up opengl
                if (GLFW.ExtensionSupported("GL_KHR_debug"))
                {
                    GL.Arb.DebugMessageCallback([DebuggerHidden] (source​, type​, id​, severity​, length​, message​, userParam) =>
                    {
                        var msg = Marshal.PtrToStringAnsi(message, length);
                        if (type == DebugType.DebugTypeError)
                            throw new InvalidOperationException(msg);
                        Console.WriteLine(msg);
                    }, nint.Zero);

                    GL.Enable(EnableCap.DebugOutput);
                    if (Debugger.IsAttached)
                        GL.Enable(EnableCap.DebugOutputSynchronous);
                }


                GL.Enable(EnableCap.FramebufferSrgb);
                GL.ClearColor(0.5f, 0.5f, 0.5f, 0);
                GL.ClearDepth(1);
                //GL.Disable(EnableCap.DepthTest);
                //GL.DepthFunc(DepthFunction.Less);
                //GL.Disable(EnableCap.CullFace);
                GL.Enable(EnableCap.CullFace);

                var hVertexShader = LoadShader(ShaderType.VertexShader, Path.GetFullPath("Shaders/defaultVertexShader.vert"));
                var hFragmentShader = LoadShader(ShaderType.FragmentShader, Path.GetFullPath("Shaders/defaultFragmentShader.frag"));

                //link shaders to a program
                hProgram = GL.CreateProgram();
                GL.AttachShader(hProgram, hFragmentShader);
                GL.AttachShader(hProgram, hVertexShader);
                GL.LinkProgram(hProgram);
                GL.GetProgram(hProgram, GetProgramParameterName.LinkStatus, out int status);
                if (status != (int)All.True)
                    throw new Exception(GL.GetProgramInfoLog(hProgram));

                cube1Node.Load(hProgram);

                GL.ActiveTexture(TextureUnit.Texture0 + tiuIndex);
                var path = Path.GetFullPath("Textures/bricks.jpg");
                var hTexture = LoadTexture(path);
                GL.BindTexture(TextureTarget.Texture2D, hTexture);

                //check for errors during all previous calls
                var error = GL.GetError();
                if (error != OpenTK.Graphics.OpenGL4.ErrorCode.NoError)
                    throw new Exception(error.ToString());
            };

            double time = 0;
            w.UpdateFrame += fea =>
            {
                time += fea.Time;


            };

            w.RenderFrame += fea =>
            {
                //clear screen and z-buffer
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                //switch to our shader
                GL.UseProgram(hProgram);

                //set uniform values
                GL.Uniform1(GL.GetUniformLocation(hProgram, "inTime"), (float)time);

                var matrix = Matrix4x4.Identity;
                matrix *= Matrix4x4.CreateRotationY((float)time * 0.5f);
                matrix *= Matrix4x4.CreateRotationX((float)time * 0.3f);
                matrix *= Matrix4x4.CreateTranslation(3, 0, -10);

                var v = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4, (float)w.Size.X / w.Size.Y, 0.1f, 100f);

                GL.Uniform1(GL.GetUniformLocation(hProgram, "brickTexture"), tiuIndex);

                root.Render(matrix, v, hProgram, (float)time);

                w.SwapBuffers();

                var error = GL.GetError();
                if (error != OpenTK.Graphics.OpenGL4.ErrorCode.NoError)
                    throw new Exception(error.ToString());
            };

            w.FramebufferResize += rea => GL.Viewport(0, 0, rea.Width, rea.Height);

            w.Run();

        }

        private static int LoadShader(ShaderType type, string path)
        {
            var source = File.ReadAllText(path);
            var shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if (status != (int)All.True)
                throw new Exception(GL.GetShaderInfoLog(shader));
            return shader;
        }
        public static int LoadTexture(string path)
        {
            using var bitmap = new Bitmap(path);
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            GL.GenTextures(1, out int hTextureObject);
            GL.BindTexture(TextureTarget.Texture2D, hTextureObject);
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)TextureMinFilter.Nearest });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)TextureMagFilter.Linear });
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Srgb8, bitmap.Width, bitmap.Height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            return hTextureObject;
        }
    }
}