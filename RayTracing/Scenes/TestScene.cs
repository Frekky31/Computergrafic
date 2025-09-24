using System.Numerics;
using RayTracing.Objects;

namespace RayTracing.Objects
{
    public class TestScene : Scene
    {
        Camera camera = new(new(0, 0, -4), new(0, 0, 6), new(0, 1, 0), 36);
        Camera camera2 = new(new(-0.9f, -0.5f, 0.9f), new(0, 0, 0), new(0, 1, 0), 110);
        Camera camera3 = new(new(0, 0, -4), new(0, 0, 6), new(0, 1, 0), 110);
        Camera cameraM = new(new(-0.9f, -0.5f, 0.9f), new(0, 0, 0), new(0, 1, 0), 101);
        int lastCam;
        int curCam;
        List<Camera> cameras;

        public TestScene()
        {
            Spheres.AddRange(
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
            ]);

            Camera = cameraM;
            cameras = [camera, camera2, camera3];
            lastCam = cameras.Count - 1;
            curCam = 0;
        }


        public int mult { get; set; } = 1;

        public float elapsedTime { get; set; } = 0;

        public override void Update(RenderTarget target, float delta)
        {
            float lingerTime = 1.0f;
            float timeToSwitch = 5.0f;
            float totalPhaseTime = 2 * timeToSwitch + 2 * lingerTime;

            float totalTime = cameras.Count * totalPhaseTime;

            elapsedTime += delta;
            
            if (elapsedTime > totalTime)
                elapsedTime -= totalTime;
            int phase = (int)(elapsedTime / totalPhaseTime);
            float phaseTime = elapsedTime - phase * totalPhaseTime;

            lastCam = phase % cameras.Count;
            curCam = (phase + 1) % cameras.Count;

            float lerpFactor = 0;
            if (phaseTime < timeToSwitch)
            {
                lerpFactor = phaseTime / timeToSwitch;
            }
            else if (phaseTime < timeToSwitch + lingerTime)
            {
                lerpFactor = 1;
            }
            else if (phaseTime < 2 * timeToSwitch + lingerTime)
            {
                lerpFactor = 1 - (phaseTime - timeToSwitch - lingerTime) / timeToSwitch;
            }
            else
            {
                lerpFactor = 0;
            }

            cameraM.Position = Vector3.Lerp(cameras[lastCam].Position, cameras[curCam].Position, lerpFactor);
            cameraM.LookAt = Vector3.Lerp(cameras[lastCam].LookAt, cameras[curCam].LookAt, lerpFactor);
            cameraM.Fov = (1 - lerpFactor) * cameras[lastCam].Fov + lerpFactor * cameras[curCam].Fov;


            if (Spheres.Count == 0)
                return;

            Sphere lastSphere = Spheres[^1];

            float speed = 0.5f;
            float newX = lastSphere.center.X + speed * delta * mult;
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

            var center = lastSphere.center;
            lastSphere.center = new Vector3(newX, center.Y, center.Z);
        }
    }
}
