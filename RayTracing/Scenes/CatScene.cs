using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using RayTracing.Core;
using RayTracing.Scenes;

namespace RayTracing.Objects
{
    public class CatScene : Scene
    {
        Camera cameraM = new(new(0, 0, -5), new(0, 0, 6), new(0, 1, 0), 36);
        static Material catMat = new(new(0.95f, 0.48f, 0.78f))
        {
            Specular = new(0.95f, 0.48f, 0.78f),
            Diffuse = new(0.95f, 0.48f, 0.78f),
            Emission = new(0.95f, 0.48f, 0.78f),
            SpecularDistance = 0.01f
        };
        Mesh cat = ObjImporter.LoadObj("Meshes/cat.obj", catMat, false);
        bool Animation = false;

        public CatScene(bool cameraScene, bool animation)
        {
            Animation = animation;
            Setup();
        }

        public CatScene()
        {
            Setup();
        }

        public void Setup()
        {
            Rectangle wallLeft = new(new(-1, -1, 1), new(0f, 2f, 0f), new(0, 0, -20), new(new(0.67f, 0.07f, 0.03f)));
            Rectangle wallRight = new(new(1, 1, 1), new(0f, -2f, 0f), new(0, 0, -20), new(new(0.09f, 0.04f, 0.67f)));
            Rectangle wallBack = new(new(-1, 1, 1), new(0f, -2f, 0f), new(2, 0, 0), new(new(0.09f, 0.6f, 0.02f)));
            Rectangle floor = new(new(1, -1, 1), new(-2f, 0f, 0f), new(0, 0, -20), new(new(0.6f, 0.6f, 0.6f)));
            Rectangle ceiling = new(new(-1, 1, 1), new(2f, 0f, 0f), new(0, 0, -20), new(new(0.6f, 0.6f, 0.6f)));

            Triangles.AddRange(wallLeft.Triangles);
            Triangles.AddRange(wallRight.Triangles);
            Triangles.AddRange(wallBack.Triangles);
            Triangles.AddRange(ceiling.Triangles);
            Triangles.AddRange(floor.Triangles);

            cat.Move(new Vector3(100f, -150.8f, 0.2f));
            cat.Rotate(Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), MathF.PI / 6));
            cat.Scale(0.003f);
            Triangles.AddRange(cat.Triangles);

            Camera = cameraM;
        }

        public float elapsedTime { get; set; } = 0;

        public override void Update(RenderTarget target, float delta)
        {
            elapsedTime += delta;
            if (Animation)
            {
                float rotationSpeed = MathF.PI * 2f / 4f;
                float angle = elapsedTime * rotationSpeed;
                cat.Rotate(Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), angle));
            }
        }
    }
}
