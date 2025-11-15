namespace Rasterization.Core
{
    public class Volume
    {
        public BoundingSphere? Sphere { get; }
        public bool HasSphere => Sphere != null;

        public Volume() { }

        public Volume(BoundingSphere sphere)
        {
            Sphere = sphere;
        }
    }
}
