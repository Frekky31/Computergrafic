using Raylib_cs;
using RayTracing.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RayTracing.Core
{
    public class Engine
    {

        public static void Run(RenderTarget target, Scene scene)
        {
            Raylib.InitWindow(800, 800, "Ray Tracing");

            Texture2D texture = Raylib.LoadTextureFromImage(Raylib.GenImageColor(target.Width, target.Height, Color.Black));
            Raylib.SetTextureFilter(texture, TextureFilter.Point);
            Color[] texColBuffer = new Color[target.Width * target.Height * 4];

            while (!Raylib.WindowShouldClose())
            {
                scene.Update(target, Raylib.GetFrameTime());

                RayTracing.Render(target, scene);


                Raylib.UpdateTexture(texture, texColBuffer);
                ToFlatByteArray(target, texColBuffer);

                Rectangle src = new(0, texture.Height, texture.Width, -texture.Height);
                Rectangle dest = new(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
                Vector2 origin = new(0, 0);

                Raylib.BeginDrawing();
                Raylib.DrawTexturePro(texture, src, dest, origin, 0.0f, Color.White);
                Raylib.DrawFPS(10, 10);
                Raylib.EndDrawing();
            }

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
