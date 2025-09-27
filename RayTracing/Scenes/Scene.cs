using RayTracing.Core;
using RayTracing.Objects;
using System.Numerics;

namespace RayTracing.Scenes
{
    public abstract class Scene
    {
        public List<Sphere> Spheres { get; } = [];
        public List<Triangle> Triangles { get; } = [];
        public List<RenderObject> Objects { get; } = [];
        public Camera Camera { get; set; } = new(new(0, 0, -4), new(0, 0, 6), new(0, 1, 0), 36);

        public abstract void Update(RenderTarget target, float delta);
    }
}
