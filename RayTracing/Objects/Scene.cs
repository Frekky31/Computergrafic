using System.Numerics;

namespace RayTracing.Objects
{
    public class Scene(List<Sphere> spheres, Camera camera)
    {
        public List<Sphere> Spheres { get; } = spheres;
        public Camera Camera { get; set; } = camera;

        public int mult { get; set; } = 1;

        public void Update(RenderTarget target, float delta)
        {
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
