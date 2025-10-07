using RayTracing.Core;
using RayTracing.Scenes;
using RayTracing.Texture;
using System.Drawing;
using System.Numerics;

namespace RayTracing.Objects
{
    public class TextureScene : Scene
    {
        Camera camera = new(new(-5, 6, -5), new(0, 1, 0), new(0, 1, 0), 36);
        Camera cameraPart2 = new(new(-0.9f, -0.5f, 0.9f), new(0, 0, 0), new(0, 1, 0), 110);

        public TextureScene()
        {
            Rectangle wallLeft = new(new(-1, -1, -1), new(0f, 2f, 0f), new(0, 0, 10), new() { Diffuse = new(0.7f, 0.07f, 0.03f) });
            Rectangle wallRight = new(new(1, -1, 1), new(0f, 2f, 0f), new(0, 0, -10), new() { Diffuse = new(0.09f, 0.04f, 0.7f) });
            Rectangle wallBack = new(new(-1, -1, 1), new(0f, 2f, 0f), new(2, 0, 0), new() { Diffuse = new(0.09f, 0.7f, 0.02f) });
            Rectangle floor = new(new(-10, 0, -10), new(20f, 0f, 0f), new(0, 0, 20), new() { Diffuse = new(0.6f, 0.6f, 0.6f) });
            Rectangle ceiling = new(new(-1, 1, 1), new(2f, 0f, 0f), new(0, 0, -10), new() { Diffuse = new(0.6f, 0.6f, 0.6f), Emission = new(2, 2f, 2f) });

            var texture = new Bitmap("Texture/bricks.jpg");
            var brickTexture = new Material() { Texture = texture };
            var brickTextureEmissive = new Material() { Texture = texture, Emission = new(1, 1, 1) };
            var brickTextureSpecular = new Material() { Texture = texture, Specular = new(1, 1, 1f), SpecularDistance = 0.06f };

            var diffuseTexture = new Material() { Diffuse = new(0.1f, 0.8f, 0.1f) };
            var specularTexture = new Material() { Diffuse = new(0.1f, 0.8f, 0.1f), Specular = new(0.1f, 0.8f, 0.1f), SpecularDistance = 0.06f };
            var specularTexture2 = new Material() { Diffuse = new(0.1f, 0.8f, 0.1f), Specular = new(1f, 1f, 1f), SpecularDistance = 0.06f };


            Material procedural = new()
            {
                ProceduralTexture = (uv) => {
                    uv = new Vector2(uv.X % 1.0f, uv.Y % 1.0f);
                    if (uv.X < 0) uv.X += 1.0f;
                    if (uv.Y < 0) uv.Y += 1.0f;

                    // Marble swirl pattern
                    float scale = 8.0f;
                    float swirl = MathF.Sin(scale * uv.X + MathF.Sin(scale * uv.Y * 2.0f));
                    float bands = MathF.Sin(scale * uv.Y + swirl * 2.0f);

                    // Color blend: blue, purple, gold
                    Vector3 colorA = new(0.2f, 0.3f, 0.8f); // blue
                    Vector3 colorB = new(0.7f, 0.2f, 0.6f); // purple
                    Vector3 colorC = new(0.9f, 0.8f, 0.2f); // gold

                    float t = 0.5f + 0.5f * bands;
                    Vector3 baseColor = Vector3.Lerp(colorA, colorB, t);
                    Vector3 finalColor = Vector3.Lerp(baseColor, colorC, MathF.Abs(swirl));

                    // Optional: add subtle noise for more realism
                    float noise = MathF.Sin(50.0f * (uv.X + uv.Y)) * 0.05f;
                    finalColor += new Vector3(noise, noise, noise);

                    // Clamp to [0,1]
                    finalColor = Vector3.Clamp(finalColor, Vector3.Zero, Vector3.One);

                    return finalColor;
                }
            };

            Material procedural2 = new()
            {
                ProceduralTexture = (uv) => {
                    // Rotate UV by 180 degrees
                    uv = new Vector2(1.0f - uv.X, 1.0f - uv.Y);

                    uv = new Vector2(uv.X % 1.0f, uv.Y % 1.0f);
                    if (uv.X < 0) uv.X += 1.0f;
                    if (uv.Y < 0) uv.Y += 1.0f;

                    var f = MathF.Sin(uv.X * MathF.PI) + MathF.Sin(uv.Y * MathF.PI);
                    return new Vector3(
                        f * 0.5f + 0.5f,
                        MathF.Sin(f * 3 + 2) * 0.5f + 0.5f,
                        MathF.Sin(f * 3 + 10) * 0.5f + 0.5f
                    );
                }
            };



            Spheres.AddRange(
            [
                //new Sphere(0.3f, new Vector3(-0.6f, -0.7f, -0.6f), circle),
                new Sphere(0.6f, new Vector3(-1.5f, 0.6f, 0f), brickTexture),
                new Sphere(0.6f, new Vector3(0f, 0.6f, 0f), brickTextureSpecular),
                new Sphere(0.6f, new Vector3(1.5f, 0.6f, 0f), brickTextureEmissive),
                new Sphere(0.6f, new Vector3(3f, 0.6f, 0f), procedural2),

                new Sphere(0.6f, new Vector3(-1.5f, 0.6f, 1.5f), diffuseTexture),
                new Sphere(0.6f, new Vector3(0f, 0.6f, 1.5f), specularTexture),
                new Sphere(0.6f, new Vector3(1.5f, 0.6f, 1.5f), specularTexture2),


                new Sphere(10, new Vector3(0, 16, -7), ceiling.Material)
            ]);

            Triangles.AddRange(floor.Triangles);



            Camera = camera;
        }

        public float elapsedTime { get; set; } = 0;

        public override void Update(RenderTarget target, float delta)
        {
            elapsedTime += delta;

        }
    }
}
