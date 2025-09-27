using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using RayTracing.Core;
using RayTracing.Scenes;

namespace RayTracing.Objects
{
    public class TestScene : Scene
    {
        Camera camera = new(new(0, 0, -5), new(0, 0, 6), new(0, 1, 0), 36);
        Camera cameraPart2 = new(new(-0.9f, -0.5f, 0.9f), new(0, 0, 0), new(0, 1, 0), 110);

        Camera cameraTop = new(new(-0.75f, 0.3f, 0f), new(-0.2f, 0f, 0f), new(0, 1, 0), 68);
        Camera cameraDolly = new(new(0, 0, -1f), new(0, 0, 6), new(0, 1, 0), 140);
        Camera cameraCube = new(new(-0.6f, -0.95f, -2f), new(0.25f, -0.75f, -1f), new(0, 1, 0), 50);

        Camera cameraM = new(new(0, 0, -5), new(0, 0, 6), new(0, 1, 0), 36);
        Sphere animatedSphere = new(0.1, new Vector3(0.3f, -0.4f, -1f), new Vector3(0.5f, 0.05f, 0.07f));
        List<Camera> cameras;
        Mesh cat = ObjImporter.LoadObj("Meshes/cat.obj", new(0.95f, 0.48f, 0.78f), true);
        bool CameraScene = false;
        bool Animation = false;

        public TestScene(bool cameraScene, bool animation)
        {
            CameraScene = cameraScene;
            Animation = animation;
            var sphA = new Sphere(0.05, new Vector3(0.5f, 0.9f, -1f), new Vector3(1f, 1f, 1f));
            var sphB = new Sphere(0.05, new Vector3(0.9f, 0.9f, -1f), new Vector3(0.5f, 0.5f, 0.5f));
            var sphC = new Sphere(0.05, new Vector3(0.9f, 0.1f, -1f), new Vector3(0.1f, 0.1f, 0.1f));
            var sphD = new Sphere(0.05, new Vector3(0.5f, 0.1f, -1f), new Vector3(0.1f, 0.1f, 0.1f));
            var sphE = new Sphere(0.05, new Vector3(0.9f, 0.9f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));
            var sphF = new Sphere(0.05, new Vector3(0.9f, 0.1f, -0.5f), new Vector3(0.1f, 0.1f, 0.1f));
            var sphG = new Sphere(0.05, new Vector3(0.5f, 0.9f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));
            var sphH = new Sphere(0.05, new Vector3(0.5f, 0.1f, -0.5f), new Vector3(0.1f, 0.1f, 0.1f));

            Cube cube = new(new Vector3(0, -0.8f, -0.6f), new Vector3(0.25f, 0.5f, 0.25f), new Vector3(0.2f, 0.3f, 0.2f));
            //cube.Rotate(Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), MathF.PI / 6));

            Rectangle wallLeft = new(new(-1, -1, 1), new(0f, 2f, 0f), new(0, 0, -20), new(0.7f, 0.07f, 0.03f));
            Rectangle wallRight = new(new(1, 1, 1), new(0f, -2f, 0f), new(0, 0, -20), new(0.09f, 0.04f, 0.7f));
            Rectangle wallBack = new(new(-1, 1, 1), new(0f, -2f, 0f), new(2, 0, 0), new(0.09f, 0.7f, 0.02f));
            Rectangle floor = new(new(1, -1, 1), new(-2f, 0f, 0f), new(0, 0, -20), new(0.8f, 0.8f, 0.8f));
            Rectangle ceiling = new(new(-1, 1, 1), new(2f, 0f, 0f), new(0, 0, -20), new(0.8f, 0.8f, 0.8f));

            //Spheres.AddRange(cube.Vertices);

            Spheres.AddRange(
            [
                //new Sphere(1000, new Vector3(-1001, 0, 0), new Vector3(0.7f, 0.07f, 0.03f)),
                //new Sphere(1000, new Vector3(1001, 0, 0), new Vector3(0.09f, 0.04f, 0.7f)),
                //new Sphere(1000, new Vector3(0, 0, 1001), new Vector3(0.4f, 0.4f, 0.4f)),
                //new Sphere(1000, new Vector3(0, -1001, 0), new Vector3(0.6f, 0.6f, 0.6f)),
                //new Sphere(1000, new Vector3(0, 1001, 0), new Vector3(0.8f, 0.8f, 0.8f)),
                
                //new Sphere(0.3, new Vector3(-0.6f, -0.7f, -0.6f), new Vector3(0.78f, 0.76f, 0.1f)),
                //new Sphere(0.6, new Vector3(0.3f, -0.4f, 0.3f), new Vector3(0.04f, 0.4f, 0.7f)),
                //new Sphere(0.8, new Vector3(0f, 1.6f, 0f), new Vector3(0.85f, 0.55f, 0.03f)),
                //animatedSphere,

                //sphA,
                //sphB,
                //sphC,
                //sphD,
                //sphE,
                //sphF,
                //sphG,
                //sphH,
            ]);

            //Triangles.AddRange(cube.Triangles);
            Triangles.AddRange(wallLeft.Triangles);
            Triangles.AddRange(wallRight.Triangles);
            Triangles.AddRange(wallBack.Triangles);
            Triangles.AddRange(ceiling.Triangles);
            Triangles.AddRange(floor.Triangles);

            var mesh = ObjImporter.LoadObj("Meshes/TestObject.obj", new(0.5f, 0.5f, 0.5f));
            var ring = ObjImporter.LoadObj("Meshes/Ring.obj", new(0.7f, 0.7f, 0.7f), true);
            mesh.Scale(0.2f);
            mesh.Move(new Vector3(0, -0.8f, -0.6f));
            ring.Scale(0.2f);
            ring.Move(new Vector3(0, -0.2f, -0.6f));
            cat.Move(new Vector3(100f, -150.8f, 0.2f));
            cat.Rotate(Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), MathF.PI / 6));
            cat.Scale(0.003f);
            //Triangles.AddRange(mesh.Triangles);
            //Triangles.AddRange(ring.Triangles);
            Triangles.AddRange(cat.Triangles);
            Triangles.AddRange(
            [
                //new Triangle(sphA.Center, sphC.Center, sphB.Center, new Vector3(0.1f, 0.5f, 0.1f)),
                //new Triangle(sphC.center, sphD.center, sphA.center, new Vector3(0.5f, 0.1f, 0.1f)),
                //new Triangle(sphB.center, sphE.center, sphF.center, new Vector3(0.5f, 0.1f, 0.1f)),
                //new Triangle(sphF.center, sphC.center, sphB.center, new Vector3(0.5f, 0.1f, 0.1f)),
                //new Triangle(sphA.center, sphH.center, sphG.center, new Vector3(0.5f, 0.1f, 0.1f)),
                //new Triangle(sphA.center, sphD.center, sphH.center, new Vector3(0.1f, 0.5f, 0.1f)),
                
            ]);



            Camera = cameraM;

            Camera camera1 = new(new(-5, 5, 0), new(0, 0, 0), new(0, 1, 0), 36);
            Camera camera2 = new(new(0, 0, 5), new(0, 0, 0), new(0, 1, 0), 36);
            Camera camera3 = new(new(5, -5, 0), new(0, 0, 0), new(0, 1, 0), 36);
            cameras = [cameraDolly, camera, cameraCube, cameraTop, cameraPart2];
            //cameras = [camera, camera1, camera2, camera3];
        }


        public int mult { get; set; } = 1;

        public float elapsedTime { get; set; } = 0;

        public override void Update(RenderTarget target, float delta)
        {
            elapsedTime += delta;

            if (CameraScene)
            {
                CameraBlend blend = GetCameraBlend(elapsedTime, 3f, 4f, cameras.Count);
                cameraM.Position = Vector3.Lerp(cameras[blend.fromIndex].Position, cameras[blend.toIndex].Position, blend.lerp);
                cameraM.LookAt = Vector3.Lerp(cameras[blend.fromIndex].LookAt, cameras[blend.toIndex].LookAt, blend.lerp);
                cameraM.Fov = (1 - blend.lerp) * cameras[blend.fromIndex].Fov + blend.lerp * cameras[blend.toIndex].Fov;
            }

            if (Animation)
            {
                float speed = 0.5f;
                float newX = animatedSphere.Center.X + speed * delta * mult;
                if (newX > 0.8f)
                {
                    newX = 0.8f;
                    mult = -1;
                }
                else if (newX < -0.8f)
                {
                    newX = -0.8f;
                    mult = 1;
                }

                // Directly modify the X component of the existing center vector
                var center = animatedSphere.Center;
                center.X = newX;
                animatedSphere.Center = center;

                // Rotate the cat mesh 360 degrees over 4 seconds
                float rotationSpeed = MathF.PI * 2f / 4f; // 360 degrees in 4 seconds
                float angle = elapsedTime * rotationSpeed;
                cat.Rotate(Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), angle));
            }
        }
        public static CameraBlend GetCameraBlend(
        float elapsedTime,
        float transitionTime,
        float lingerTime,
        int cameraCount)
        {
            if (cameraCount <= 0) return new CameraBlend { fromIndex = -1, toIndex = -1, lerp = 0f };

            if (transitionTime <= 0f) // instant cut, only linger
                transitionTime = 0.000001f; // avoid div-by-zero; treat as nearly instant

            if (lingerTime < 0f) lingerTime = 0f;
            if (elapsedTime < 0f) elapsedTime = 0f;

            float segmentDuration = transitionTime + lingerTime;
            if (segmentDuration <= 0f) // degenerate: no time passes, stick to first camera
                return new CameraBlend { fromIndex = 0, toIndex = (cameraCount > 1 ? 1 % cameraCount : 0), lerp = 1f };

            float cycleDuration = segmentDuration * cameraCount;
            float tCycle = elapsedTime % cycleDuration;

            int segmentIndex = (int)Math.Floor(tCycle / segmentDuration);
            int fromIndex = segmentIndex % cameraCount;
            int toIndex = (fromIndex + 1) % cameraCount;

            float tInSegment = tCycle - segmentIndex * segmentDuration;

            float lerp = tInSegment < transitionTime
                ? tInSegment / transitionTime
                : 1f; // linger phase

            return new CameraBlend { fromIndex = fromIndex, toIndex = toIndex, lerp = lerp };
        }

    }

    public struct CameraBlend
    {
        public int fromIndex;
        public int toIndex;
        public float lerp; // 0..1
    }
}
