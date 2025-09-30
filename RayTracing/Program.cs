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
            int width = 500;
            int height = 500;
            RenderTarget renderTarget = new(width, height);

            RayTracer rayTracing = new()
            {
                SamplesPerPixel = 32,
                MaxRayBounces = 1000,
                TileSize = 20,
                BounceChance = 0.1f
            };

            Engine.Run(renderTarget, new OnlySpheres(), rayTracing, false);
        }
    }
}
