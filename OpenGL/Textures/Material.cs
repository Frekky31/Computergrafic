using System.Numerics;
using OpenTK.Graphics.OpenGL4;

namespace OpenGL.Objects
{
    public class Material
    {
        public Vector3 Color { get; set; } = new Vector3(1f, 1f, 1f);

        // OpenGL texture handle (0 = none)
        public int TextureHandle { get; set; } = 0;

        // If true the texture contains an alpha channel and the object should be treated as (partially) transparent
        public bool HasTextureAlpha { get; set; } = false;

        public float Shininess { get; set; } = 32f;            // specular exponent
        public float SpecularStrength { get; set; } = 0.5f;    // specular multiplier

        public void Apply(int hProgram, int textureUnitIndex = 0)
        {
            if (hProgram == 0) return;

            var locColor = GL.GetUniformLocation(hProgram, "material.color");
            if (locColor != -1)
                GL.Uniform3(locColor, Color.X, Color.Y, Color.Z);

            var locShine = GL.GetUniformLocation(hProgram, "material.shininess");
            if (locShine != -1)
                GL.Uniform1(locShine, Shininess);

            var locSpec = GL.GetUniformLocation(hProgram, "material.specularStrength");
            if (locSpec != -1)
                GL.Uniform1(locSpec, SpecularStrength);

            var locHasTex = GL.GetUniformLocation(hProgram, "material.hasTexture");
            if (locHasTex != -1)
                GL.Uniform1(locHasTex, TextureHandle != 0 ? 1 : 0);

            var locTex = GL.GetUniformLocation(hProgram, "material.texture");
            if (locTex != -1)
            {
                if (TextureHandle != 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + textureUnitIndex);
                    GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
                    GL.Uniform1(locTex, textureUnitIndex);
                }
                else
                {
                    // If no texture, set sampler to 0 but don't bind
                    GL.Uniform1(locTex, textureUnitIndex);
                }
            }
        }
    }
}