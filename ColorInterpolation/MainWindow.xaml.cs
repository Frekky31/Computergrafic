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

namespace ColorInterpolation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int ImageWidth = 300;
        private const int ImageHeight = 300;
        private WriteableBitmap bitmap;
        private Vector3[] pixelColors;

        private Vector3 Color1 = new(0.5f, 0.3f, 1f);
        private Vector3 Color2 = new(0.75f, 0.6f, 0.01f);

        public MainWindow()
        {
            InitializeComponent();
            InitializeImage();
            RenderVector3ColorsToBitmap();
            imgDisplay.Source = bitmap;
        }

        private void InitializeImage()
        {
            // Initialize WriteableBitmap
            bitmap = new WriteableBitmap(ImageWidth, ImageHeight, 96, 96, PixelFormats.Bgra32, null);

            pixelColors = new Vector3[ImageWidth * ImageHeight];

            for (int i = 0; i < ImageHeight; i++)
            {
                for (int j = 0; j < ImageWidth; j++)
                {
                    int index = i * ImageWidth + j;
                    float t = (float)j / (ImageWidth - 1);
                    pixelColors[index] = Vector3.Lerp(Color1, Color2, t);
                }
            }
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
    }
}
