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
using RayTracing.Texture;

namespace ColorInterpolation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int ImageWidth = 300;
        private const int ImageHeight = 300;
        private readonly WriteableBitmap bitmap = new(ImageWidth, ImageHeight, 96, 96, PixelFormats.Bgra32, null);
        private readonly Vector3[] pixelColors = new Vector3[ImageWidth * ImageHeight];

        private Vector3 Color1 = new(0f, 1f, 1f);
        private Vector3 Color2 = new(0f, 1f, 0f);

        public MainWindow()
        {
            InitializeComponent();
            PrintImage();
        }

        private void PrintImage()
        {
            Interpolation();
            RenderVector3ColorsToBitmap();
            imgDisplay.Source = bitmap;
        }

        private void Interpolation()
        {
            for (int i = 0; i < ImageHeight; i++)
            {
                for (int j = 0; j < ImageWidth; j++)
                {
                    int index = i * ImageWidth + j;

                    float u = (float)j / (ImageWidth - 1f);
                    float v = (float)i / (ImageHeight - 1f);

                    var uv = new Vector2(u, v);
                    pixelColors[index] = GetColor(uv);
                }
            }
        }

        public Vector3 GetColor(Vector2 uv)
        {

            // x and y are expected to be in the range [0, 1]

            var f = MathF.Sin(uv.X * 30) + MathF.Sin(uv.Y * 30);
            return new Vector3(
                f * 0.5f + 0.5f,
                MathF.Sin(f * 3 + 2) * 0.5f + 0.5f,
                MathF.Sin(f * 3 + 10) * 0.5f + 0.5f
            );
            return WoodTexture.Sample(uv, 0.01f);
            // Map x and y from [0, ImageWidth-1] and [0, ImageHeight-1] to [0, 1]
            float scale = 0.07f;

            // Smoother: average several nearby samples
            float v = CellularNoise2D.Cellular(uv * scale);

            return new Vector3(v);
        }

        public Vector3 GetColorSin(float x, float y)
        {
            var f = MathF.Sin(x / 10f + MathF.Sin(y / 40f) * 5) * 5;
            return new(MathF.Sin(f) * 0.5f + 0.5f, MathF.Sin(f + 2) * 0.5f + 0.5f, MathF.Sin(f + 10) * 0.5f + 0.5f);
        }

        private void RenderVector3ColorsToBitmap()
        {
            byte[] pixels = new byte[ImageWidth * ImageHeight * 4];

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

            Int32Rect rect = new(0, 0, ImageWidth, ImageHeight);
            bitmap.WritePixels(rect, pixels, ImageWidth * 4, 0);
        }


        private static byte FloatToSrgbByte(float c)
        {
            c = Math.Clamp(c, 0f, 1f);

            c = (float)Math.Pow(c, 1.0 / 2.2);

            c = Math.Clamp(c, 0f, 1f);

            return (byte)(c * 255);
        }

        private static float SrgbByteToFloat(byte c)
        {
            float f = c / 255f;

            f = (float)Math.Pow(f, 2.2);

            f = Math.Clamp(f, 0f, 1f);

            return f;
        }

        private void ColorPicker1_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                Color1.X = SrgbByteToFloat(e.NewValue.Value.R);
                Color1.Y = SrgbByteToFloat(e.NewValue.Value.G);
                Color1.Z = SrgbByteToFloat(e.NewValue.Value.B);
                PrintImage();
            }
        }

        private void ColorPicker2_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                Color2.X = SrgbByteToFloat(e.NewValue.Value.R);
                Color2.Y = SrgbByteToFloat(e.NewValue.Value.G);
                Color2.Z = SrgbByteToFloat(e.NewValue.Value.B);
                PrintImage();
            }
        }
    }

    public enum Orientation
    {
        Vertical, Horizontal, Diagonal
    }
}
