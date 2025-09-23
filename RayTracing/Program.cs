using RayTracing.Core;
using RayTracing.Objects;
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
            List<Sphere> spheres =
            [
                new Sphere(1000, new Vector3(-1001, 0, 0), new Vector3(0.7f, 0.07f, 0.03f)),
                new Sphere(1000, new Vector3(1001, 0, 0), new Vector3(0.09f, 0.04f, 0.7f)),
                new Sphere(1000, new Vector3(0, 0, 1001), new Vector3(0.4f, 0.4f, 0.4f)),
                new Sphere(1000, new Vector3(0, -1001, 0), new Vector3(0.6f, 0.6f, 0.6f)),
                new Sphere(1000, new Vector3(0, 1001, 0), new Vector3(0.8f, 0.8f, 0.8f)),

                new Sphere(0.3, new Vector3(-0.6f, -0.7f, -0.6f), new Vector3(0.78f, 0.76f, 0.1f)),
                new Sphere(0.6, new Vector3(0.3f, -0.4f, 0.3f), new Vector3(0.04f, 0.4f, 0.7f)),
                new Sphere(0.8, new Vector3(-0.8f, 0.8f, 0.8f), new Vector3(0.85f, 0.55f, 0.03f)),

                new Sphere(0.1, new Vector3(0.3f, -0.4f, -1f), new Vector3(0.5f, 0.05f, 0.07f)),
            ];

            Camera camera = new(new(0, 0, -4), new(0, 0, 6), new(0, 1, 0), 36);
            Camera camera2 = new(new(-0.9f, -0.5f, 0.9f), new(0, 0, 0), new(0, 1, 0), 101);

            Scene scene = new(spheres, camera);

            RenderTarget renderTarget = new(700, 700);

            Engine.Run(renderTarget, scene);
        }
    }
}
