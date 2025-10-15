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
            RenderTarget renderTarget = new(900, 900);

            Stopwatch watch = new();
            RayTracer rayTracer = new()
            {
                SamplesPerPixel = 1024,
                MaxDepth = 20,
                Probability = 0.25f,
                UseBVH = false,
                UseBRDF = true,
                ProgressCallback = (current, total) =>
                    {
                        var elapsedMs = watch.ElapsedMilliseconds;
                        var sec = Math.DivRem(elapsedMs, 1000, out long ms);
                        Console.WriteLine($"{DateTime.Now:HH:mm:ss} - Progress: {current} / {total} pixels ({current * 100 / total}% - {sec}.{ms})");
                    }
            };
            
            watch.Start();
            Engine.Run(renderTarget, new SpheresScene(), rayTracer);
            watch.Stop();
        }
    }
}
