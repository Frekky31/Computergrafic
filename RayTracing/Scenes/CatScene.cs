using RayTracing.Core;
using RayTracing.Scenes;
using RayTracing.Texture;
using System.Drawing;
using System.Numerics;

namespace RayTracing.Objects
{
    public class CatScene : Scene
    {
        Camera camera = new(new(3, 2, 3), new(0, 1f, 0), new(0, 1, 0), 45);

        public CatScene()
        {
            var (hdrData, hdrWidth, hdrHeight) = HDRLoader.LoadEXR("Texture/horn-koppe_spring_4k.hdr");
            var hdrMaterial = new Material
            {
                HDRTexture = hdrData,
                HDRWidth = hdrWidth,
                HDRHeight = hdrHeight,
                Emission = Vector3.One * 1.2f
            };


            Spheres.AddRange(
            [
                new Sphere(20f, new Vector3(0, 0f, 0f), hdrMaterial),
                //new Sphere(10, new Vector3(4, 18, 9), ceiling)
            ]);

            Material catMat = new()
            {
                Specular = new(0.97f, 0.002f, 0.298f),
                Diffuse = new(0.97f, 0.002f, 0.298f),
                Emission = new(0.97f, 0.002f, 0.298f),
                SpecularDistance = 0.01f
            };
            Mesh cat = ObjImporter.LoadObj("Meshes/cat.obj", catMat, false);
            cat.Scale(0.005f);
            cat.Move(new Vector3(0f, 1f, 0f));

            Rectangle floor = new(new(-10, 0, -10), new(20f, 0f, 0f), new(0, 0, 20), new() { Diffuse = new(0.5f, 0.5f, 0.5f) });
            Triangles.AddRange(floor.Triangles);
            Triangles.AddRange(cat.Triangles);

            Camera = camera;
        }

        public float elapsedTime { get; set; } = 0;

        public override void Update(RenderTarget target, float delta)
        {
            elapsedTime += delta;

        }
    }
}
