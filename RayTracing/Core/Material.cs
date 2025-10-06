using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Core
{
    public class Material
    {
        public Vector3 Diffuse { get; set; }
        public Vector3 Emission { get; set; }
        public Vector3 Specular { get; set; }

        public float SpecularDistance { get; set; } = 0.01f;

        public Material() {
            Diffuse = Vector3.Zero;
            Emission = Vector3.Zero;
            Specular = Vector3.Zero;
        }

        public Material(Vector3 diffuse)
        {
            Diffuse = diffuse;
            Emission = Vector3.Zero;
            Specular = Vector3.Zero;
        }

        public Material(Vector3 diffuse, Vector3 emission)
        {
            Diffuse = diffuse;
            Emission = emission;
            Specular = Vector3.Zero;
        }

        public Material(Vector3 diffuse, Vector3 emission, Vector3 specular)
        {
            Diffuse = diffuse;
            Emission = emission;
            Specular = specular;
        }

        public Material(Vector3 diffuse, Vector3 emission, Vector3 specular, float specularDist)
        {
            Diffuse = diffuse;
            Emission = emission;
            Specular = specular;
            SpecularDistance = specularDist;
        }
    }
}
