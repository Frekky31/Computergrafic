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
        public Vector3 Diffuse { get; set; } = Vector3.Zero;
        public Vector3 Specular { get; set; } = Vector3.Zero;
        public Vector3 Emission { get; set; } = Vector3.Zero;
        public float SpecularDistance { get; set; } = 0.01f;

        public Material() { }
        public Material(Vector3 diffuse)
        {
            Diffuse = diffuse;
        }

        public Material(Vector3 diffuse, Vector3 emission)
        {
            Diffuse = diffuse;
            Emission = emission;
        }
        
        public Material(Vector3 diffuse, Vector3 specular, float specularDistance = 0.01f)
        {
            Diffuse = diffuse;
            Specular = specular;
            SpecularDistance = specularDistance;
        }

        public Material(Vector3 diffuse, Vector3 specular, Vector3 emission, float specularDistance = 0.01f)
        {
            Diffuse = diffuse;
            Specular = specular;
            Emission = emission;
            SpecularDistance = specularDistance;
        }
    }
}
