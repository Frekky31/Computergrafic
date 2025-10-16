using RayTracing.Core;
using RayTracing.Objects;
using RayTracing.Scenes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing
{
    public class Program
    {
        static void Main(string[] args)
        {
            RenderTarget renderTarget = new(800, 800);
            Stopwatch watch = new();
            RayTracer rayTracer = new()
            {
                SamplesPerPixel = 8192,
                MaxDepth = 20,
                Probability = 0.2f,
                UseBVH = true,
                UseBRDF = true,
                ProgressCallback = (current, total) =>
                {
                    var elapsedMs = watch.ElapsedMilliseconds;
                    var sec = Math.DivRem(elapsedMs, 1000, out long ms);
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss} - Progress: {current} / {total} pixels ({current * 100 / total}% - {sec}.{ms})");
                }
            };

            // List of scenes to render
            var scenes = new List<(Scene scene, string name)>
            {
                (new SpheresScene(), "spheres"),
                (new TextureScene(), "texture"),
                (new ProceduralScene(), "procedural"),
                (new CatScene(), "cat"),
            };

            Engine.Run(renderTarget);

            foreach (var (scene, name) in scenes)
            {
                renderTarget.Clear(rayTracer.BackgroundColor);
                watch.Restart();
                rayTracer.Render(renderTarget, scene);
                watch.Stop();
                Engine.Show(renderTarget);
                string fileName = $"Pictures/{DateTime.Now:yyyyMMddHHmmss}_{name}.png";
                renderTarget.SaveToFile(fileName);
                Console.WriteLine($"Saved: {fileName}");
            }
            Engine.EventLoop();
        }
    }
}
