using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using RayTracing.Core;
using RayTracing.Scenes;

namespace RayTracing.Objects
{
    public class OnlySpheres : Scene
    {
        Camera cameraM = new(new(0, 0, -5), new(0, 0, 6), new(0, 1, 0), 36);

        public OnlySpheres()
        {
            Material light = new()
            {
                Emission = new Vector3(2, 2, 2)
            };

            Material m_wallLeft = new(new(0.7f, 0.07f, 0.03f));
            Material m_wallRight = new(new(0.09f, 0.04f, 0.7f));
            Material m_wallBack = new(new(0.6f, 0.6f, 0.6f));
            Material m_floor = new(new(0.6f, 0.6f, 0.6f));


            Material m_cube1 = new() { Specular = new(0.78f, 0.76f, 0.1f), Diffuse= new(0.78f, 0.76f, 0.1f), SpecularDistance = 0.01f };
            Material m_cube2 = new() { Specular = new(1f, 1f, 1f), Diffuse = new(0.8f, 0.8f, 0.8f), SpecularDistance = 0.01f };

            Spheres.AddRange(
            [
                new Sphere(1000, new Vector3(-1001, 0, 0), m_wallLeft),
                new Sphere(1000, new Vector3(1001, 0, 0), m_wallRight),
                new Sphere(1000, new Vector3(0, 0, 1001), m_wallBack),
                new Sphere(1000, new Vector3(0, -1001, 0), m_floor),
                new Sphere(1000, new Vector3(0, 1001, 0), light),

                new Sphere(0.3f, new Vector3(-0.6f, -0.69f, -0.6f), m_cube1),
                new Sphere(0.6f, new Vector3(0.3f, -0.39f, 0.3f), m_cube2),
            ]);


            Camera = cameraM;
        }



        public override void Update(RenderTarget target, float delta)
        {
            
        }

    }
}
