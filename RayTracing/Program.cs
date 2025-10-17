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
using System.Xml.Linq;

namespace RayTracing
{
    public class Program
    {
        static void Main(string[] args)
        {
            RenderTarget renderTarget = new(500, 500);
            Stopwatch watch = new();
            RayTracer rayTracerStrong = new()
            {
                SamplesPerPixel = 512,
                MaxDepth = 20,
                Probability = 0.2f,
                UseBVH = false,
                UseBRDF = true,
                ProgressCallback = (current, total) =>
                {
                    var elapsedMs = watch.ElapsedMilliseconds;
                    var sec = Math.DivRem(elapsedMs, 1000, out long ms);
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss} - Progress: {current} / {total} pixels ({current * 100 / total}% - {sec}.{ms})");
                }
            };

            RayTracer rayTracerWeak = new()
            {
                SamplesPerPixel = 1024,
                MaxDepth = 20,
                Probability = 0.18f,
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
                //(new SpheresScene(), "spheres"),
                //(new TextureScene(), "texture"),
                (new ProceduralScene(), "procedural"),
                //(new CatScene(), "cat"),
            };

            Engine.Run(renderTarget);

            foreach (var (scene, name) in scenes)
            {
                watch.Restart();
                rayTracerStrong.Render(renderTarget, scene);
                watch.Stop();
                Engine.Show(renderTarget);
                renderTarget.SaveToFile($"Pictures/{DateTime.Now:yyyyMMddHHmmss}_{name}.png");
                Console.WriteLine($"Saved: {$"Pictures/{DateTime.Now:yyyyMMddHHmmss}_{name}.png"}");
            }

            //watch.Restart();
            //rayTracerWeak.Render(renderTarget, new CatScene());
            //watch.Stop();
            //Engine.Show(renderTarget);
            //renderTarget.SaveToFile($"Pictures/{DateTime.Now:yyyyMMddHHmmss}_cat.png");
            //Console.WriteLine($"Saved: {$"Pictures/{DateTime.Now:yyyyMMddHHmmss}_cat.png"}");

            Engine.EventLoop();
        }
    }
}
