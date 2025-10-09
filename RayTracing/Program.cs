using RayTracing.Core;
using RayTracing.Objects;
using RayTracing.Scenes;
using System;
using System.Collections.Generic;
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

            RayTracer rayTracer = new()
            {
                SamplesPerPixel = 32,
                MaxDepth = 20,
                Probability = 0.25f,
                ProgressCallback = (current, total) =>
                    {
                        Console.WriteLine($"Progress: {current} / {total} pixels ({current * 100 / total}%)");
                    }
            };

            Engine.Run(renderTarget, new ProceduralScene(), rayTracer);
        }
    }
}
