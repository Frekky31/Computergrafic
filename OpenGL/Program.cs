using OpenGL.Objects;
using OpenGL.Scenes;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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

            MainScene scene = new MainScene();

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
                GL.Enable(EnableCap.DepthTest);
                GL.DepthMask(true);
                GL.DepthFunc(DepthFunction.Less);
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
                
                scene.LoadScene(hProgram);

                var error = GL.GetError();
                if (error != OpenTK.Graphics.OpenGL4.ErrorCode.NoError)
                    throw new Exception(error.ToString());
            };

            double time = 0;
            w.UpdateFrame += fea =>
            {
                time += fea.Time;
                scene.UpdateScene(hProgram, (float)time);
            };

            w.RenderFrame += fea =>
            {
                //clear screen and z-buffer
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                //switch to our shader
                GL.UseProgram(hProgram);

                //set uniform values
                GL.Uniform1(GL.GetUniformLocation(hProgram, "inTime"), (float)time);

                GL.Uniform1(GL.GetUniformLocation(hProgram, "brickTexture"), tiuIndex);

                scene.RenderScene(hProgram, (float)time, w);

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