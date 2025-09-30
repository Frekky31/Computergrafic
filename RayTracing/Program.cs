using RayTracing.Core;
using RayTracing.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RayTracing
{
    public class Program
    {
        static void Main(string[] args)
        {
            float aspectRatio = 16f / 9f;
            int width = 600;
            int height = 600;
            RenderTarget renderTarget = new(width, height);

            RayTracer rayTracing = new()
            {
                SamplesPerPixel = 128,
                MaxRayBounces = 100000,
                TileSize = 20,
                BounceChance = 0.01f
            };

            Engine.Run(renderTarget, new CatScene(), rayTracing, false);
        }
    }
}
