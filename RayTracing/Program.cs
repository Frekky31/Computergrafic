using RayTracing.Core;
using RayTracing.Objects;
using RayTracing.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing
{
    public class Program
    {
        static void Main(string[] args)
        {
            RenderTarget renderTarget = new(600, 600);

            RayTracer rayTracer = new()
            {
                SamplesPerPixel = 32,
                MaxDepth = 20,
                Probability = 0.25f,
                ProgressCallback = (current, total) =>
                    {
                        System.Diagnostics.Debug.WriteLine($"Progress: {current} / {total} rays ({current * 100 / total}%)");
                    }
            };

            Engine.Run(renderTarget, new TextureScene(), rayTracer);
        }
    }
}
