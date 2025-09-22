using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Objects
{
    internal class Scene
    {
        private List<Sphere> spheres = [];
        public Scene() { }
        // In your Scene type, add:
        public List<PointLight> Lights = new();

            // Helper if you like:
        public IEnumerable<PointLight> GetLights() => Lights;
        public void AddSphere(Sphere sphere)
        {
            spheres.Add(sphere);
        }

        public List<Sphere> GetSpheres()
        {
            return spheres;
        }
    }
}
