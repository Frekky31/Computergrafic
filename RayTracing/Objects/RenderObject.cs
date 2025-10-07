using RayTracing.Texture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Objects
{
    public abstract class RenderObject
    {
        protected RenderObject() { }

        public abstract Span<Triangle> GetTriangles();
        public abstract Span<Sphere> GetSpheres();
        public Material Material { get; set; } = new Material();

        public abstract void Move(Vector3 translation);

        public abstract void Rotate(Quaternion rotation);
        public abstract void Scale(float scale);
    }
}
