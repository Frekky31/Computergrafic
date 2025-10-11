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
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        static void Main(string[] args)
        {
            AllocConsole();
            RenderTarget renderTarget = new(600, 600);

            Stopwatch watch = new();
            RayTracer rayTracer = new()
            {
                SamplesPerPixel = 16,
                MaxDepth = 20,
                Probability = 0.25f,
                UseBVH = true,
                UseBRDF = true,
                ProgressCallback = (current, total) =>
                    {
                        var elapsedMs = watch.ElapsedMilliseconds;
                        var sec = Math.DivRem(elapsedMs, 1000, out long ms);
                        Console.WriteLine($"{DateTime.Now:HH:mm:ss} - Progress: {current} / {total} pixels ({current * 100 / total}% - {sec}.{ms})");
                    }
            };
            
            watch.Start();
            Engine.Run(renderTarget, new ProceduralScene(), rayTracer);
            watch.Stop();
        }
    }
}
