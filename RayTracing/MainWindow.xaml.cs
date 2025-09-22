using RayTracing.Objects;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RayTracing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Scene Scene = new();
        private Vector3 up = Vector3.Normalize(new(0, 1, 0));
        private readonly WriteableBitmap bitmap = new(700, 700, 96, 96, PixelFormats.Bgra32, null);
        private static readonly Random rng = new Random(1337);
        int maxBounces = 8;        // recursion cap
        int samplesPerPixel = 32;   // try 1..32; higher = smoother
        const float EPS = 1e-3f;


        public MainWindow()
        {
            InitializeComponent();

            Objects.Camera a = new()
            {
                Position = new(0, 0, -4),
                LookAt = new(0, 0, 6),
                Up = Vector3.Normalize(new(0, 1, 0)),
                Fov = 36
            };

            Objects.Camera b = new()
            {
                Position = new(-0.9f, -0.5f, 0.9f),
                LookAt = new(0, 0, 0),
                Up = Vector3.Normalize(new(0, 1, 0)),
                Fov = 110
            };

            Scene.AddSphere(new Sphere(1000, new Vector3(-1001, 0, 0), new Vector3(0.7f, 0.07f, 0.03f)));
            Scene.AddSphere(new Sphere(1000, new Vector3(1001, 0, 0), new Vector3(0.03f, 0.04f, 0.8f)));
            Scene.AddSphere(new Sphere(1000, new Vector3(0, 0, 1001), new Vector3(0.4f, 0.4f, 0.4f)));
            Scene.AddSphere(new Sphere(1000, new Vector3(0, -1001, 0), new Vector3(0.6f, 0.6f, 0.6f)));
            Scene.AddSphere(new Sphere(1000, new Vector3(0, 1001, 0), new Vector3(0.8f, 0.8f, 0.8f)));

            Scene.AddSphere(new Sphere(0.3, new Vector3(-0.6f, -0.7f, -0.6f), new Vector3(0.78f, 0.7f, 0.18f)));
            Scene.AddSphere(new Sphere(0.6, new Vector3(0.3f, -0.4f, 0.3f), new Vector3(0.14f, 0.6f, 0.8f)));

            Scene.Lights.Add(new PointLight
            {
                position = new Vector3(0f, 0.9f, 0f),
                color = new Vector3(0.87f, 0.86f, 0.76f),
                intensity = 2f
            });
            CreateRays(a);
        }


        private void CreateRays(Objects.Camera cam)
        {
            Vector3[] pixelColors = new Vector3[(int)(bitmap.Width * bitmap.Height)];
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Vector3 accum = Vector3.Zero;

                    for (int s = 0; s < samplesPerPixel; s++)
                    {
                        // Subpixel jitter for anti-aliasing / sampling
                        float jx = NextFloat();
                        float jy = NextFloat();

                        float px = x + jx;
                        float py = y + jy;

                        Vector2 pixel = new(
                            -((2f * px) / (float)(bitmap.Width - 1) - 1f) * (float)bitmap.Width / (float)bitmap.Height,
                            1f - 2f * py / (float)(bitmap.Height - 1)
                        );

                        var (o, d) = CreateEyeRay(cam, pixel);

                        // Full path trace from the eye ray:
                        Vector3 color = TraceDiffuse(Scene, o, d, 0);
                        accum += color;
                    }

                    Vector3 finalColor = accum / (float)samplesPerPixel;

                    int index = y * (int)bitmap.Width + x;
                    pixelColors[index] = finalColor;
                }
            }
            RenderVector3ColorsToBitmap(pixelColors, (int)bitmap.Width, (int)bitmap.Height, bitmap);
            imgDisplay.Source = bitmap;
        }

        private (Vector3 o, Vector3 d) CreateEyeRay(Objects.Camera cam, Vector2 pixel)
        {
            Vector3 f = Vector3.Normalize(cam.LookAt - cam.Position);
            Vector3 r = Vector3.Normalize(Vector3.Cross(f, up));
            Vector3 u = Vector3.Normalize(Vector3.Cross(r, f));
            float scale = (float)Math.Tan((cam.Fov * MathF.PI / 180f) / 2);
            float beta = scale * pixel.Y;
            float omega = scale * pixel.X;

            Vector3 d = Vector3.Normalize(f + beta * u + omega * r);

            return (cam.Position, d);
        }

        private HitPoint? FindClosestHitPoint(Scene s, Vector3 o, Vector3 d)
        {

            List<HitPoint> hits = [];
            foreach (var sphere in s.GetSpheres())
            {
                Vector3 oc = o - sphere.center;
                float b = 2.0f * Vector3.Dot(oc, d);
                float c = Vector3.Dot(oc, oc) - (float)(sphere.radius * sphere.radius);
                float discriminant = b * b - 4 * c;

                if (discriminant < 0)
                    continue;

                float t1 = (-b - (float)Math.Sqrt(discriminant)) / 2.0f;
                float t2 = (-b + (float)Math.Sqrt(discriminant)) / 2.0f;

                if (t1 > 0)
                    hits.Add(GetHitPoint(o, d, t1, sphere));
                if (t2 > 0)
                    hits.Add(GetHitPoint(o, d, t2, sphere));
            }

            if (hits.Count > 0)
            {
                hits.Sort((a, b) => a.Distance.CompareTo(b.Distance));
                return hits[0];
            }
            return null;
        }

        private HitPoint GetHitPoint(Vector3 o, Vector3 d, float t, Sphere sphere)
        {
            Vector3 hitPoint = o + t * d;
            Vector3 normal = Vector3.Normalize(hitPoint - sphere.center);
            return new HitPoint { DidHit = true, Distance = t, Point = hitPoint, Color = sphere.color, Normal = normal };
        }


        private Vector3 ComputeColor(Scene scene, Vector3 o, Vector3 d, HitPoint hit, int depth, int maxBounces)
        {
            if (depth >= maxBounces)
                return hit.Color; // stop recursion, just return base color

            // Example: simple reflection
            Vector3 reflectionDir = Vector3.Reflect(d, hit.Normal);
            var newHit = FindClosestHitPoint(scene, hit.Point + 0.001f * reflectionDir, reflectionDir);

            if (newHit != null && newHit.DidHit)
            {
                // Mix surface color with reflected color
                Vector3 reflectedColor = ComputeColor(scene, hit.Point, reflectionDir, newHit, depth + 1, maxBounces);
                return 0.2f * hit.Color + 0.8f * reflectedColor; // tweak mix ratio
            }

            // No further hit → return surface color
            return hit.Color;
        }



        private Vector3 Background(Vector3 d)
        {
            // Simple sky: lerp white -> light blue by up direction
            float t = 0.5f * (d.Y + 1f);
            return (1f - t) * new Vector3(1f, 1f, 1f) + t * new Vector3(0.5f, 0.7f, 1f);
        }

        private float NextFloat()
        {
            // uniform [0,1)
            return (float)rng.NextDouble();
        }

        private void BuildOrthonormalBasis(Vector3 n, out Vector3 t, out Vector3 b)
        {
            // Robust tangent/binormal from normal
            Vector3 up = MathF.Abs(n.Z) < 0.999f ? new Vector3(0, 0, 1) : new Vector3(1, 0, 0);
            t = Vector3.Normalize(Vector3.Cross(up, n));
            b = Vector3.Normalize(Vector3.Cross(n, t));
        }

        // Cosine-weighted hemisphere direction around normal n
        private Vector3 SampleCosineHemisphere(Vector3 n)
        {
            float r1 = NextFloat();
            float r2 = NextFloat();

            float phi = 2f * MathF.PI * r1;
            float r = MathF.Sqrt(r2);
            float x = r * MathF.Cos(phi);
            float y = r * MathF.Sin(phi);
            float z = MathF.Sqrt(MathF.Max(0f, 1f - r2)); // points up in local frame

            BuildOrthonormalBasis(n, out var t, out var b);
            // Transform from local (x,y,z) to world
            return Vector3.Normalize(x * t + y * b + z * n);
        }


        private Vector3 TraceDiffuse(Scene scene, Vector3 o, Vector3 d, int depth)
        {
            if (depth >= maxBounces)
                return Background(d);

            var hit = FindClosestHitPoint(scene, o, d);
            if (hit == null || !hit.DidHit)
                return Background(d);

            var h = hit;
            Vector3 p = h.Point + EPS * h.Normal;
            Vector3 albedo = h.Color;

            // 1) Direct lighting (shadow rays to point lights)
            Vector3 direct = ComputeDirectLights(scene, p, h.Normal, albedo);

            // 2) Indirect diffuse bounce (cosine-weighted hemisphere)
            Vector3 bounceDir = SampleCosineHemisphere(h.Normal);
            Vector3 incoming = TraceDiffuse(scene, p, bounceDir, depth + 1);

            // With cosine-weighted sampling, the indirect estimator simplifies to albedo * incoming
            Vector3 indirect = new Vector3(
                albedo.X * incoming.X,
                albedo.Y * incoming.Y,
                albedo.Z * incoming.Z
            );

            return direct + indirect;
        }




        private Vector3 ComputeDirectLights(Scene scene, Vector3 p, Vector3 n, Vector3 albedo)
        {
            Vector3 sum = Vector3.Zero;

            foreach (var light in scene.GetLights())
            {
                // Vector from hit point to light
                Vector3 L = light.position - p;
                float dist2 = Vector3.Dot(L, L);
                if (dist2 <= 0f) continue;

                float dist = MathF.Sqrt(dist2);
                Vector3 ldir = L / dist;

                // Shadow ray: if anything is hit before the light, this light is occluded
                var shadowHit = FindClosestHitPoint(scene, p + EPS * ldir, ldir);
                bool occluded = shadowHit != null && shadowHit.DidHit && shadowHit.Distance < dist - EPS;
                if (occluded) continue;

                // Lambertian BRDF = albedo / PI, cosine = max(0, n·l)
                float NdotL = MathF.Max(0f, Vector3.Dot(n, ldir));
                if (NdotL <= 0f) continue;

                // Inverse-square falloff
                float attenuation = light.intensity / dist2;

                // Li from the light (radiance), scaled by attenuation and light color
                Vector3 Li = attenuation * light.color;

                // Direct contribution: albedo/PI * N·L * Li
                Vector3 contrib = (albedo / MathF.PI) * NdotL * Li;

                sum += contrib;
            }

            return sum;
        }



        private static void RenderVector3ColorsToBitmap(Vector3[] pixelColors, int imageWidth, int imageHeight, WriteableBitmap bitmap)
        {
            byte[] pixels = new byte[imageWidth * imageHeight * 4];

            for (int i = 0; i < pixelColors.Length; i++)
            {
                Vector3 linearColor = pixelColors[i];

                byte r = FloatToSrgbByte(linearColor.X);
                byte g = FloatToSrgbByte(linearColor.Y);
                byte b = FloatToSrgbByte(linearColor.Z);
                byte a = 255;

                int baseIndex = i * 4;
                pixels[baseIndex + 0] = b;
                pixels[baseIndex + 1] = g;
                pixels[baseIndex + 2] = r;
                pixels[baseIndex + 3] = a;
            }

            Int32Rect rect = new(0, 0, imageWidth, imageHeight);
            bitmap.WritePixels(rect, pixels, imageWidth * 4, 0);
        }


        private static byte FloatToSrgbByte(float c)
        {
            c = Math.Clamp(c, 0f, 1f);

            c = (float)Math.Pow(c, 1.0 / 2.2);

            c = Math.Clamp(c, 0f, 1f);

            return (byte)(c * 255);
        }
    }

    public struct PointLight
    {
        public Vector3 position;   // world-space
        public Vector3 color;      // RGB in [0,1] (light tint)
        public float intensity;    // luminous power scalar (try 10–100)
    }


}