using RayTracing.Core;
using RayTracing.Scenes;
using RayTracing.Texture;
using System.Drawing;
using System.Numerics;

namespace RayTracing.Objects
{
    public class ProceduralScene : Scene
    {
        Camera camera = new(new(7, 5, 7), new(0, 2.5f, 0), new(0, 1, 0), 45);

        public ProceduralScene()
        {
            Material procedural1 = new()
            {
                ProceduralTexture = (uv) =>
                {
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
                ProceduralTexture = (uv) =>
                {
                    // Rotate UV by 180 degrees
                    uv = new Vector2(1.0f - uv.X, 1.0f - uv.Y);

                    uv = new Vector2(uv.X % 1.0f, uv.Y % 1.0f);
                    if (uv.X < 0) uv.X += 1.0f;
                    if (uv.Y < 0) uv.Y += 1.0f;
                    var f = MathF.Sin(uv.X * 10f + MathF.Sin(uv.Y * 5f) * 5) * 5;
                    return new(
                        MathF.Sin(f) * 0.5f + 0.5f,
                        MathF.Sin(f + 2) * 0.5f + 0.5f,
                        MathF.Sin(f + 10) * 0.5f + 0.5f
                    );
                }
            };

            Material procedural3 = new()
            {
                ProceduralTexture = (uv) =>
                {
                    return LavaLampTexture.Sample(uv);
                }
            };

            Material procedural4 = new()
            {
                ProceduralTexture = (uv) =>
                {
                    return WoodTexture.Sample(uv, 1f);
                }
            };

            Material procedural5 = new()
            {
                ProceduralTexture = (uv) =>
                {
                    return BoxShader.Sample(uv, new(1, 1), 1500f);
                }
            };

            Material procedural6 = new()
            {
                ProceduralTexture = (uv) =>
                {
                    return TilePattern.Sample(uv, new(0.6f, 0.6f), 14f);
                }
            };

            Material procedural7 = new()
            {
                ProceduralTexture = (uv) =>
                {
                    uv *= 10.0f;

                    float v = CellularNoise2D.Cellular(uv);
                    return new Vector3(
                        MathF.Sin(3.0f * v + 0.0f),
                        MathF.Sin(3.0f * v + 2.0f),
                        MathF.Sin(3.0f * v + 4.0f)
                    ) * 0.5f + new Vector3(0.5f);
                }
            };
            Material procedural8 = new()
            {
                Emission = Vector3.One,
                ProceduralTexture = (uv) =>
                {
                    // Rotate UV by 180 degrees
                    uv = new Vector2(1.0f - uv.X, 1.0f - uv.Y);

                    uv = new Vector2(uv.X % 1.0f, uv.Y % 1.0f);
                    if (uv.X < 0) uv.X += 1.0f;
                    if (uv.Y < 0) uv.Y += 1.0f;
                    var f = MathF.Sin(uv.X * 30) + MathF.Sin(uv.Y * 30);
                    return new Vector3(
                        f * 0.5f + 0.5f,
                        MathF.Sin(f * 3 + 2) * 0.5f + 0.5f,
                        MathF.Sin(f * 3 + 10) * 0.5f + 0.5f
                    );
                }
            };
            
            Material procedural9 = new()
            {
                Emission = Vector3.One * 0.5f,
                ProceduralTexture = (uv) =>
                {
                    uv *= 5.4f;
                    Vector2 frag = new Vector2(uv.X + 3.5f, uv.Y + 3.5f);
                    Vector2 iChannel0_rg = new Vector2(0.5f, 0.5f);
                    return StarfieldShader.Sample(frag, new Vector2(1000, 800), iChannel0_rg);
                }
            };

            var (hdrData, hdrWidth, hdrHeight) = HDRLoader.LoadEXR("Texture/horn-koppe_spring_4k.hdr");
            var hdrMaterial = new Material
            {
                HDRTexture = hdrData,
                HDRWidth = hdrWidth,
                HDRHeight = hdrHeight,
                Emission = Vector3.One * 1.2f
            };


            Spheres.AddRange(
            [
                new Sphere(0.6f, new Vector3(1.5f, 0.6f, 0.5f), procedural1),
                new Sphere(0.6f, new Vector3(0f, 0.6f, 0.5f), procedural2),
                new Sphere(0.6f, new Vector3(-1.5f, 0.6f, 0.5f), procedural3),
                new Sphere(0.6f, new Vector3(-3f, 0.6f, 0.5f), procedural4),

                new Sphere(0.6f, new Vector3(1.5f, 0.6f, -1f), procedural5),
                new Sphere(0.6f, new Vector3(0f, 0.6f, -1f), procedural6),
                new Sphere(0.6f, new Vector3(-1.5f, 0.6f, -1f), procedural7),
                new Sphere(0.6f, new Vector3(-3f, 0.6f, -1f), procedural8),
                
                new Sphere(0.6f, new Vector3(1.5f, 0.6f, -2.5f), procedural9),

                new Sphere(20f, new Vector3(0, 0f, 0f), hdrMaterial),
                //new Sphere(10, new Vector3(4, 18, 9), ceiling)
            ]);

            Material catMat = new()
            {
                Specular = new(0.97f, 0.002f, 0.298f),
                Diffuse = new(0.97f, 0.002f, 0.298f),
                SpecularDistance = 0.01f
            };
            Mesh cat = ObjImporter.LoadObj("Meshes/cat.obj", catMat, false);
            cat.Scale(0.003f);
            cat.Move(new Vector3(-1f, 0.6f, -5f));
            cat.Rotate(Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), MathF.PI / 6));

            Rectangle floor = new(new(-10, 0, -10), new(20f, 0f, 0f), new(0, 0, 20), new() { Diffuse = new(0.5f, 0.5f, 0.5f) });
            Triangles.AddRange(floor.Triangles);
            //Triangles.AddRange(cat.Triangles);

            Camera = camera;
        }

        public float elapsedTime { get; set; } = 0;

        public override void Update(RenderTarget target, float delta)
        {
            elapsedTime += delta;

        }
    }
}
