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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RayTracing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Vector3 eye = new(0, 0, -4);
        private Vector3 eye1 = new(-0.9f, -0.5f, 0.9f);

        private Vector3 lookAt = new(0, 0, 6);
        private Vector3 lookAt1 = new(0, 0, 0);

        private float fov = 50;
        private Scene Scene = new();
        private Vector3 up = Vector3.Normalize(new(0, 1, 0));
        private readonly WriteableBitmap bitmap = new(700, 700, 96, 96, PixelFormats.Bgra32, null);

        public MainWindow()
        {
            InitializeComponent();
            Scene.AddSphere(new Sphere(1000, new Vector3(-1001, 0, 0), new Vector3(0.7f, 0.07f, 0.03f)));
            Scene.AddSphere(new Sphere(1000, new Vector3(1001, 0, 0), new Vector3(0.09f, 0.04f, 0.7f)));
            Scene.AddSphere(new Sphere(1000, new Vector3(0, 0, 1001), new Vector3(0.4f, 0.4f, 0.4f)));
            Scene.AddSphere(new Sphere(1000, new Vector3(0, -1001, 0), new Vector3(0.6f, 0.6f, 0.6f)));
            Scene.AddSphere(new Sphere(1000, new Vector3(0, 1001, 0), new Vector3(0.8f, 0.8f, 0.8f)));

            Scene.AddSphere(new Sphere(0.3, new Vector3(-0.6f, -0.7f, -0.6f), new Vector3(0.78f, 0.7f, 0.01f)));
            Scene.AddSphere(new Sphere(0.6, new Vector3(0.3f, -0.4f, 0.3f), new Vector3(0.04f, 0.4f, 0.7f)));

            Scene.AddSphere(new Sphere(0.1, new Vector3(0.3f, -0.4f, -1f), new Vector3(0f, 0f, 0f)));

            CreateRays();
        }


        private void CreateRays()
        {
            Vector3[] pixelColors = new Vector3[(int)(bitmap.Width * bitmap.Height)];
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Vector2 pixel = new(
                        -((2f * x) / (float)(bitmap.Width - 1) - 1f) * (float)bitmap.Width / (float)bitmap.Height, // negative X
                        1f - 2f * y / (float)(bitmap.Height - 1)
                    );
                    var (o, d) = CreateEyeRay(eye, lookAt, fov, pixel);
                    var hit = FindClosestHitPoint(Scene, o, d);
                    if (hit != null && hit.DidHit)
                    {
                        int index = y * (int)bitmap.Width + x;
                        pixelColors[index] = ComputeColor(Scene, o, d, hit);
                    }
                }
            }
            RenderVector3ColorsToBitmap(pixelColors, (int)bitmap.Width, (int)bitmap.Height, bitmap);
            imgDisplay.Source = bitmap;
        }

        private (Vector3 o, Vector3 d) CreateEyeRay(Vector3 eye, Vector3 lookAt, float fov, Vector2 pixel)
        {
            Vector3 f = Vector3.Normalize(lookAt - eye);
            Vector3 r = Vector3.Normalize(Vector3.Cross(f, up));
            Vector3 u = Vector3.Normalize(Vector3.Cross(r, f));
            float scale = (float)Math.Tan((fov * MathF.PI / 180f) / 2);
            float beta = scale * pixel.Y;
            float omega = scale * pixel.X;

            Vector3 d = Vector3.Normalize(f + beta * u + omega * r);

            return (eye, d);
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


        private Vector3 ComputeColor(Scene scene, Vector3 o, Vector3 d, HitPoint hit)
        {
            
            return hit.Color;
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
}