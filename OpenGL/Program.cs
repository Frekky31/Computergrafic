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
            int vaoTriangle = 0;
            int vboTriangleIndices = 0;
            int tiuIndex = 1;

            var cube1 = Mesh.CreateCube(new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 1, 0), new(1, 0, 1), new(0, 1, 1));
            var cube2 = Mesh.CreateCube(new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 1, 0), new(1, 0, 1), new(0, 1, 1));

            List<float> vertexList = [];
            List<float> texCoordList = [];
            List<float> colorList = [];

            // prepare vertex and color lists with cube1 and cube2 data
            
            foreach (var v in cube1.Vertices)
            {
                vertexList.Add(v.Position.X);
                vertexList.Add(v.Position.Y);
                vertexList.Add(v.Position.Z);
                colorList.Add(v.Color.X);
                colorList.Add(v.Color.Y);
                colorList.Add(v.Color.Z);
                texCoordList.Add(v.TexCoord.X);
                texCoordList.Add(v.TexCoord.Y);
            }

            foreach (var v in cube2.Vertices)
            {
                vertexList.Add(v.Position.X);
                vertexList.Add(v.Position.Y);
                vertexList.Add(v.Position.Z);
                colorList.Add(v.Color.X);
                colorList.Add(v.Color.Y);
                colorList.Add(v.Color.Z);
                texCoordList.Add(v.TexCoord.X);
                texCoordList.Add(v.TexCoord.Y);
            }

            var indices = cube1.Tris.SelectMany(t => new[] { t.A, t.C, t.B }).ToArray();
            int indexCount = indices.Length;

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


                //upload model vertices to a vbo
                var vboTriangleVertices = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboTriangleVertices);
                GL.BufferData(BufferTarget.ArrayBuffer, vertexList.Count * sizeof(float), vertexList.ToArray(), BufferUsageHint.StaticDraw);

                // upload model indices to a vbo
                vboTriangleIndices = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboTriangleIndices);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

                // upload model colors to a vbo
                var vboColors = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboColors);
                GL.BufferData(BufferTarget.ArrayBuffer, colorList.Count * sizeof(float), colorList.ToArray(), BufferUsageHint.StaticDraw);

                var vboTexCoords = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboTexCoords);
                GL.BufferData(BufferTarget.ArrayBuffer, texCoordList.Count * sizeof(float), texCoordList.ToArray(), BufferUsageHint.StaticDraw);

                //set up a vao
                vaoTriangle = GL.GenVertexArray();
                GL.BindVertexArray(vaoTriangle);
                var posAttribIndex = GL.GetAttribLocation(hProgram, "inPos");
                if (posAttribIndex != -1)
                {
                    GL.EnableVertexAttribArray(posAttribIndex);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vboTriangleVertices);
                    GL.VertexAttribPointer(posAttribIndex, 3, VertexAttribPointerType.Float, false, 0, 0);
                }

                var colorAttribIndex = GL.GetAttribLocation(hProgram, "inColor");
                if (colorAttribIndex != -1)
                {
                    GL.EnableVertexAttribArray(colorAttribIndex);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vboColors);
                    GL.VertexAttribPointer(colorAttribIndex, 3, VertexAttribPointerType.Float, false, 0, 0);
                }

                var texCoordAttribIndex = GL.GetAttribLocation(hProgram, "inTexCoord");
                if (texCoordAttribIndex != -1)
                {
                    GL.EnableVertexAttribArray(texCoordAttribIndex);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vboTexCoords);
                    GL.VertexAttribPointer(texCoordAttribIndex, 2, VertexAttribPointerType.Float, false, 0, 0);
                }

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
                //process logic

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
                matrix *= Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4, (float)w.Size.X / w.Size.Y, 0.1f, 100f);
                GL.UniformMatrix4(GL.GetUniformLocation(hProgram, "inMatrix"), 1, false, ref matrix.M11);

                GL.Uniform1(GL.GetUniformLocation(hProgram, "brickTexture"), tiuIndex);

                //render our model
                GL.BindVertexArray(vaoTriangle);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboTriangleIndices);
                GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, 0);

                //display
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