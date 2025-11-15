using RayTracing.Core;
using RayTracing.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Scenes
{
    internal class CatBox : Scene
    {

        public CatBox()
        {
            Material light = new()
            {
                Emission = new Vector3(2, 2, 2)
            };

            Material m_wallLeft = new() { Diffuse = new(0.7f, 0.07f, 0.03f), Specular = new(0.7f, 0.07f, 0.03f),  SpecularDistance = 0.01f };
            Material m_wallRight = new() { Diffuse = new(0.09f, 0.04f, 0.7f), Specular = new(0.09f, 0.04f, 0.7f),  SpecularDistance = 0.01f };
            Material m_wallBack = new() { Diffuse = new(0.03f, 0.76f, 0.06f), Specular = new(0.03f, 0.76f, 0.06f),  SpecularDistance = 0.01f };
            Material m_floor = new() { Diffuse = new(0.6f, 0.6f, 0.6f), Specular = new(0.6f, 0.6f, 0.6f),  SpecularDistance = 0.01f };


            Material m_cube1 = new() { Specular = new(0.78f, 0.76f, 0.1f), Diffuse = new(0.78f, 0.76f, 0.1f), SpecularDistance = 0.01f };
            Material m_cube2 = new() { Specular = new(1f, 1f, 1f), Diffuse = new(0.8f, 0.8f, 0.8f), SpecularDistance = 0.01f };

            Spheres.AddRange(
            [
                new Sphere(1000, new Vector3(-1001, 0, 0), m_wallLeft),
                new Sphere(1000, new Vector3(1001, 0, 0), m_wallRight),
                new Sphere(1000, new Vector3(0, 0, 1001), m_wallBack),
                new Sphere(1000, new Vector3(0, -1001, 0), m_floor),
                new Sphere(1000, new Vector3(0, 1001, 0), m_floor),
            ]);

            Material catMat = new()
            {
                Specular = new(0.97f, 0.002f, 0.298f),
                Diffuse = new(0.97f, 0.002f, 0.298f),
                Emission = new(0.97f, 0.002f, 0.298f),
                SpecularDistance = 0.01f
            };
            Mesh cat = MeshLoader.LoadMesh("Meshes/cat.obj", catMat, false);
            cat.Scale(0.003f);
            cat.Rotate(Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), MathF.PI / 4));
            cat.Move(new Vector3(0.15f, -0.46f, 0f));

            Triangles.AddRange(cat.Triangles);

            Camera camera1 = new(new(0, 0, -5), new(0, 0, 6), new(0, 1, 0), 36);
            Camera camera2 = new(new(-0.9f, -0.5f, 0.9f), new(0, 0, 0), new(0, 1, 0), 110);
            Camera = camera1;
        }
    }
}
