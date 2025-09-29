using System.Numerics;
using RayTracing.Core;
using RayTracing.Scenes;

namespace RayTracing.Objects
{
    public class TestScene : Scene
    {
        Camera camera = new(new(0, 0, -5), new(0, 0, 6), new(0, 1, 0), 36);
        Camera cameraPart2 = new(new(-0.9f, -0.5f, 0.9f), new(0, 0, 0), new(0, 1, 0), 110);
        Camera cameraDolly = new(new(0, 0, -1.5f), new(0, 0, 6), new(0, 1, 0), 140);
        Camera cameraLeft = new(new(-5, 2, 0), new(0.25f, -0.25f, -1f), new(0, 1, 0), 36);
        Camera cameraBack = new(new(0, 5, 5), new(0.25f, -0.25f, -1f), new(0, 1, 0), 36);
        Camera cameraRight = new(new(5, 3, 0), new(0.25f, -0.25f, -1f), new(0, 1, 0), 36);

        Camera cameraM = new(new(0, 0, -5), new(0, 0, 6), new(0, 1, 0), 36);
        Sphere animatedSphere = new Sphere(0.1, new Vector3(0.3f, -0.4f, -1f), new(new(0.7f, 0.07f, 0.03f), Vector3.Zero, Vector3.Zero));
        List<Camera> cameras;

        bool CameraScene = false;
        bool Animation = false;

        public TestScene(bool cameraScene, bool animation)
        {
            CameraScene = cameraScene;
            Animation = animation;
            //var sphA = new Sphere(0.05, new Vector3(0.5f, 0.9f, -1f), new Vector3(1f, 1f, 1f));
            //var sphB = new Sphere(0.05, new Vector3(0.9f, 0.9f, -1f), new Vector3(0.5f, 0.5f, 0.5f));
            //var sphC = new Sphere(0.05, new Vector3(0.9f, 0.1f, -1f), new Vector3(0.1f, 0.1f, 0.1f));
            //var sphD = new Sphere(0.05, new Vector3(0.5f, 0.1f, -1f), new Vector3(0.1f, 0.1f, 0.1f));
            //var sphE = new Sphere(0.05, new Vector3(0.9f, 0.9f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));
            //var sphF = new Sphere(0.05, new Vector3(0.9f, 0.1f, -0.5f), new Vector3(0.1f, 0.1f, 0.1f));
            //var sphG = new Sphere(0.05, new Vector3(0.5f, 0.9f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));
            //var sphH = new Sphere(0.05, new Vector3(0.5f, 0.1f, -0.5f), new Vector3(0.1f, 0.1f, 0.1f));

            //Cube cube = new(new Vector3(0.25f, -0.75f, -1f), new Vector3(0.25f, 0.25f, 0.25f), new Vector3(0.2f, 0.3f, 0.2f));
            
            
            Rectangle wallLeft = new(new(-1, -1, -1), new(0f, 2f, 0f), new(0, 0, 10), new(new(0.7f, 0.07f, 0.03f), Vector3.Zero, Vector3.Zero));
            Rectangle wallRight = new(new(1, -1, 1), new(0f, 2f, 0f), new(0, 0, -10), new(new(0.09f, 0.04f, 0.7f), Vector3.Zero, Vector3.Zero));
            Rectangle wallBack = new(new(-1, -1, 1), new(0f, 2f, 0f), new(2, 0, 0), new(new(0.09f, 0.7f, 0.02f), Vector3.Zero, Vector3.Zero));
            Rectangle floor = new(new(-1, -1, 1), new(2f, 0f, 0f), new(0, 0, -10), new(new(0.6f, 0.6f, 0.6f), Vector3.Zero, Vector3.Zero));
            Rectangle ceiling = new(new(-1, 1, 1), new(2f, 0f, 0f), new(0, 0, -10), new(new(0.6f, 0.6f, 0.6f), new(1,1,1), Vector3.Zero));

            //Spheres.AddRange(cube.ToSpheres());
            Spheres.AddRange(
            [
                new Sphere(1000, new Vector3(-1001, 0, 0), wallLeft.Material),
                new Sphere(1000, new Vector3(1001, 0, 0), wallRight.Material),
                new Sphere(1000, new Vector3(0, 0, 1001), wallBack.Material),
                new Sphere(1000, new Vector3(0, -1001, 0), floor.Material),
                new Sphere(1000, new Vector3(0, 1001, 0), ceiling.Material),
                new Sphere(0.3, new Vector3(-0.6f, -0.7f, -0.6f), new(new(0.78f, 0.76f, 0.1f), Vector3.Zero, new(0.78f, 0.76f, 0.1f), 0.5f)),
                new Sphere(0.6, new Vector3(0.3f, -0.4f, 0.3f), new(new(0.04f, 0.4f, 0.7f), Vector3.Zero, new(0.04f, 0.4f, 0.7f), 0.5f)),
                //new Sphere(0.8, new Vector3(-0.8f, 0.8f, 0.8f), new Vector3(0.85f, 0.55f, 0.03f)),
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

            //Triangles.AddRange(cube.ToTriangles(true));
            //Triangles.AddRange(wallLeft.ToTriangles());
            //Triangles.AddRange(wallRight.ToTriangles());
            //Triangles.AddRange(wallBack.ToTriangles());
            //Triangles.AddRange(ceiling.ToTriangles()); 
            //Triangles.AddRange(floor.ToTriangles());
            Triangles.AddRange(
            [
                //new Triangle(sphA.center, sphB.center, sphC.center, new Vector3(0.1f, 0.5f, 0.1f)),
                //new Triangle(sphC.center, sphD.center, sphA.center, new Vector3(0.5f, 0.1f, 0.1f)),
                //new Triangle(sphB.center, sphE.center, sphF.center, new Vector3(0.5f, 0.1f, 0.1f)),
                //new Triangle(sphF.center, sphC.center, sphB.center, new Vector3(0.5f, 0.1f, 0.1f)),
                //new Triangle(sphA.center, sphH.center, sphG.center, new Vector3(0.5f, 0.1f, 0.1f)),
                //new Triangle(sphA.center, sphD.center, sphH.center, new Vector3(0.1f, 0.5f, 0.1f)),
                
            ]);



            Camera = cameraM;
            cameras = [camera, cameraPart2, cameraDolly];
        }


        public int mult { get; set; } = 1;

        public float elapsedTime { get; set; } = 0;

        public override void Update(RenderTarget target, float delta)
        {
            elapsedTime += delta;

            if (CameraScene)
            {
                CameraBlend blend = GetCameraBlend(elapsedTime, 0f, 6f, cameras.Count);
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

                var center = animatedSphere.Center;
                center.X = newX;
                animatedSphere.Center = center;
            }
        }
        public static CameraBlend GetCameraBlend(
        float elapsedTime,
        float transitionTime,
        float lingerTime,
        int cameraCount)
        {
            if (cameraCount <= 0) return new CameraBlend { fromIndex = -1, toIndex = -1, lerp = 0f };

            if (transitionTime <= 0f)
                transitionTime = 0.000001f;

            if (lingerTime < 0f) lingerTime = 0f;
            if (elapsedTime < 0f) elapsedTime = 0f;

            float segmentDuration = transitionTime + lingerTime;
            if (segmentDuration <= 0f)
                return new CameraBlend { fromIndex = 0, toIndex = (cameraCount > 1 ? 1 % cameraCount : 0), lerp = 1f };

            float cycleDuration = segmentDuration * cameraCount;
            float tCycle = elapsedTime % cycleDuration;

            int segmentIndex = (int)System.Math.Floor(tCycle / segmentDuration);
            int fromIndex = segmentIndex % cameraCount;
            int toIndex = (fromIndex + 1) % cameraCount;

            float tInSegment = tCycle - segmentIndex * segmentDuration;

            float lerp = tInSegment < transitionTime
                ? tInSegment / transitionTime
                : 1f;

            return new CameraBlend { fromIndex = fromIndex, toIndex = toIndex, lerp = lerp };
        }

    }

    public struct CameraBlend
    {
        public int fromIndex;
        public int toIndex;
        public float lerp;
    }
}
