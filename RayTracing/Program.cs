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
            int width = 1080;
            int height = (int)(width / aspectRatio);
            RenderTarget renderTarget = new(width, height);

            Engine.Run(renderTarget, new CatScene(false, false), false);
        }
    }
}
