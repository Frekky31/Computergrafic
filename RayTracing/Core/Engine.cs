using Raylib_cs;
using RayTracing.Scenes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;

namespace RayTracing.Core
{
    public class Engine
    {
        public static void Run(RenderTarget target, Scene scene, bool run = false)
        {
            Run(target, scene, new RayTracer(), run);
        }

        public static void Run(RenderTarget target, Scene scene, RayTracer rayTracer, bool run = false)
        {
            Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
            Raylib.InitWindow(target.Width, target.Height, "Ray Tracing");

            Image img = Raylib.GenImageColor(target.Width, target.Height, Color.Black);
            Texture2D texture = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);
            Raylib.SetTextureFilter(texture, TextureFilter.Point);

            Color[] texColBuffer = new Color[target.Width * target.Height * 4];
            Rectangle src = new(0, texture.Height, texture.Width, -texture.Height);
            Vector2 origin = new(0, 0);
            bool firstFrameDrawn = false;

            while (!Raylib.WindowShouldClose())
            {
                if (run || !firstFrameDrawn)
                {
                    if (run)
                        scene.Update(target, Raylib.GetFrameTime());

                    rayTracer.Render(target, scene);
                    ToFlatByteArray(target, texColBuffer);
                    Raylib.UpdateTexture(texture, texColBuffer);

                    Rectangle dest = new(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
                    Raylib.BeginDrawing();
                    Raylib.DrawTexturePro(texture, src, dest, origin, 0.0f, Color.White);
                    Raylib.EndDrawing();

                    firstFrameDrawn = true;
                    if (!run)
                    {
                        if(!Directory.Exists("Pictures"))
                            Directory.CreateDirectory("Pictures");
                        Image screenshot = Raylib.LoadImageFromTexture(texture);

                        Raylib.ImageFlipVertical(ref screenshot);

                        Raylib.ExportImage(screenshot, $"Pictures/output-{DateTime.Now:yyyyMMddHHmm}.png");
                        Raylib.UnloadImage(screenshot);
                    }
                }
                else
                {
                    Raylib.PollInputEvents();
                    Raylib.WaitTime(0.05f);
                }
            }

            Raylib.UnloadTexture(texture);
            Raylib.CloseWindow();
        }

        static void ToFlatByteArray(RenderTarget renderTarget, Color[] data)
        {
            Parallel.For(0, renderTarget.Height * renderTarget.Width, i =>
            {
                Vector3 col = renderTarget.ColourBuffer[i];
                data[i] = new Color(FloatToSrgbByte(col.X), FloatToSrgbByte(col.Y), FloatToSrgbByte(col.Z), 255);
            });
        }

        private static int FloatToSrgbByte(float c)
        {
            c = (float)Math.Pow(c, 1.0 / 2.2);

            c = Math.Clamp(c, 0f, 1f);

            return (int)(c * 255);
        }
    }
}
