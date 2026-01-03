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
using System.Drawing.Imaging;
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

            ManyObjectsScene scene = new ManyObjectsScene();

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

                // enable alpha blending so textures with alpha (like cloud.png) render correctly
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

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
            // Load image, preserve alpha if present, upload as sRGB (with alpha when needed)
            using var original = new Bitmap(path);

            // Detect alpha presence
            bool hasAlpha = System.Drawing.Image.IsAlphaPixelFormat(original.PixelFormat);

            // Ensure we have a bitmap with a known packed format for LockBits
            var texBmp = new Bitmap(original.Width, original.Height, hasAlpha ? System.Drawing.Imaging.PixelFormat.Format32bppArgb : System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(texBmp))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(original, 0, 0, original.Width, original.Height);
            }

            System.Drawing.Imaging.PixelFormat lockFormat = hasAlpha ? System.Drawing.Imaging.PixelFormat.Format32bppArgb : System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            var rect = new Rectangle(0, 0, texBmp.Width, texBmp.Height);
            var data = texBmp.LockBits(rect, ImageLockMode.ReadOnly, lockFormat);

            try
            {
                GL.GenTextures(1, out int hTextureObject);
                GL.BindTexture(TextureTarget.Texture2D, hTextureObject);

                // Filtering and wrapping
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                // Use trilinear mipmapping for minification
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);

                // Pixel store alignment: 1 is safe for tightly packed rows (good for 24bpp), keep 1 to be safe
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

                // Choose internal and pixel formats depending on alpha presence
                PixelInternalFormat internalFormat = hasAlpha ? PixelInternalFormat.Srgb8Alpha8 : PixelInternalFormat.Srgb8;
                OpenTK.Graphics.OpenGL4.PixelFormat inputFormat = hasAlpha ? OpenTK.Graphics.OpenGL4.PixelFormat.Bgra : OpenTK.Graphics.OpenGL4.PixelFormat.Bgr; // GDI+ stores 32bpp as BGRA and 24bpp as BGR
                PixelType inputType = PixelType.UnsignedByte;

                GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, texBmp.Width, texBmp.Height, 0, inputFormat, inputType, data.Scan0);

                // Generate mipmaps so textures are MIP mapped
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                // Optionally set max LOD if desired:
                // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLod, 8);

                return hTextureObject;
            }
            finally
            {
                texBmp.UnlockBits(data);
                texBmp.Dispose();
            }
        }
    }
}