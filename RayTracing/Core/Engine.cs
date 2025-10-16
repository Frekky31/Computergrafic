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
        private static Texture2D texture;
        private static Color[]? texColBuffer;
        private static Rectangle src;
        private static Vector2 origin;
        private static bool windowOpen = false;

        public static void Run(RenderTarget target)
        {
            if (windowOpen) return;
            Raylib.SetConfigFlags(ConfigFlags.VSyncHint | ConfigFlags.ResizableWindow);
            Raylib.InitWindow(target.Width, target.Height, "Ray Tracing");
            windowOpen = true;

            Image img = Raylib.GenImageColor(target.Width, target.Height, Color.Black);
            texture = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);
            Raylib.SetTextureFilter(texture, TextureFilter.Point);

            texColBuffer = new Color[target.Width * target.Height * 4];
            src = new(0, texture.Height, texture.Width, -texture.Height);
            origin = new(0, 0);
        }

        public static void EventLoop()
        {
            // Keeps the window open and processes events
            while (windowOpen && !Raylib.WindowShouldClose())
            {
                Raylib.PollInputEvents();
                Raylib.WaitTime(0.05f);
            }
            if (windowOpen)
            {
                Raylib.UnloadTexture(texture);
                Raylib.CloseWindow();
                windowOpen = false;
            }
        }

        public static void Show(RenderTarget target)
        {
            if (!windowOpen) return;
            Raylib.SetWindowSize(target.Width, target.Height);
            ToFlatByteArray(target, texColBuffer);
            Raylib.UpdateTexture(texture, texColBuffer);

            Rectangle dest = new(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
            Raylib.BeginDrawing();
            Raylib.DrawTexturePro(texture, src, dest, origin, 0.0f, Color.White);
            Raylib.EndDrawing();
        }

        static void ToFlatByteArray(RenderTarget renderTarget, Color[]? data)
        {
            if (data == null) return;
            Parallel.For(0, renderTarget.Height * renderTarget.Width, i =>
            {
                Vector3 col = renderTarget.ColourBuffer[i];
                data[i] = new Color(FloatToSrgbByte(col.X), FloatToSrgbByte(col.Y), FloatToSrgbByte(col.Z), 255);
            });
        }

        private static int FloatToSrgbByte(float c)
        {
            c = Math.Clamp(c, 0f, 1f);
            c = MathF.Pow(c, 1.0f / 2.2f);
            c = Math.Clamp(c, 0f, 1f);
            return (int)(c * 255);
        }
    }
}
