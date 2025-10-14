using RayTracing.Core;
using System.Drawing;
using System.Numerics;



internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Post Processing File:");
        string filePath = Console.ReadLine().Replace("\"", "") ?? "";
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File does not exist. Press any Key...");
            Console.ReadKey();
            return;
        }

        Bitmap bitmap = new(filePath);
        int width = bitmap.Width;
        int height = bitmap.Height;
        RenderTarget target = new(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = bitmap.GetPixel(x, y);
                target.ColourBuffer[y * width + x] = new Vector3(MathF.Pow(pixel.R / 255f, 2.2f), MathF.Pow(pixel.G / 255f, 2.2f), MathF.Pow(pixel.B / 255f, 2.2f));
            }
        }


        ApplyGaussianBlur(target, 2, 0.4f);
        ApplyBilateralFilter(target, radius: 4, sigmaSpatial: 2.0f, sigmaColor: 0.4f);


        Bitmap output = new(width, height);
        Graphics g = Graphics.FromImage(output);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 col = target.ColourBuffer[y * width + x];
                col = Vector3.Clamp(col, Vector3.Zero, Vector3.One);
                col = new Vector3(MathF.Pow(col.X, 1f / 2.2f), MathF.Pow(col.Y, 1f / 2.2f), MathF.Pow(col.Z, 1f / 2.2f));
                output.SetPixel(x, y, Color.FromArgb(
                    (int)(col.X * 255),
                    (int)(col.Y * 255),
                    (int)(col.Z * 255)
                ));
            }
        }
        g.Dispose();
        string outputPath = Path.Combine(Path.GetDirectoryName(filePath) ?? "", $"{Path.GetFileName(filePath)}_output.png");
        output.Save(outputPath);
        Console.WriteLine($"Output saved to {outputPath}. Press any Key...");
        Console.ReadKey();
    }

    public static void ApplyGaussianBlur(RenderTarget target, int radius, float sigma)
    {
        int width = target.Width;
        int height = target.Height;
        var buffer = target.ColourBuffer;
        var temp = new Vector3[width * height];

        // Create Gaussian kernel
        float[] kernel = new float[2 * radius + 1];
        float sum = 0f;
        for (int i = -radius; i <= radius; i++)
        {
            float value = MathF.Exp(-(i * i) / (2 * sigma * sigma));
            kernel[i + radius] = value;
            sum += value;
        }
        // Normalize kernel
        for (int i = 0; i < kernel.Length; i++)
            kernel[i] /= sum;

        // Horizontal pass
        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 acc = Vector3.Zero;
                for (int k = -radius; k <= radius; k++)
                {
                    int sx = Math.Clamp(x + k, 0, width - 1);
                    acc += buffer[y * width + sx] * kernel[k + radius];
                }
                temp[y * width + x] = acc;
            }
        });

        // Vertical pass
        Parallel.For(0, width, x =>
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 acc = Vector3.Zero;
                for (int k = -radius; k <= radius; k++)
                {
                    int sy = Math.Clamp(y + k, 0, height - 1);
                    acc += temp[sy * width + x] * kernel[k + radius];
                }
                buffer[y * width + x] = acc;
            }
        });
    }

    public static void ApplyBilateralFilter(RenderTarget target, int radius, float sigmaSpatial, float sigmaColor)
    {
        int width = target.Width;
        int height = target.Height;
        var buffer = target.ColourBuffer;
        var temp = new Vector3[width * height];

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 center = buffer[y * width + x];
                Vector3 sum = Vector3.Zero;
                float wsum = 0f;

                for (int dy = -radius; dy <= radius; dy++)
                {
                    int ny = Math.Clamp(y + dy, 0, height - 1);
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        int nx = Math.Clamp(x + dx, 0, width - 1);
                        Vector3 neighbor = buffer[ny * width + nx];

                        float spatialDist2 = dx * dx + dy * dy;
                        float spatialWeight = MathF.Exp(-spatialDist2 / (2 * sigmaSpatial * sigmaSpatial));

                        float colorDist2 = Vector3.DistanceSquared(center, neighbor);
                        float colorWeight = MathF.Exp(-colorDist2 / (2 * sigmaColor * sigmaColor));

                        float weight = spatialWeight * colorWeight;
                        sum += neighbor * weight;
                        wsum += weight;
                    }
                }
                temp[y * width + x] = wsum > 0 ? sum / wsum : center;
            }
        });

        // Copy result back to buffer
        Array.Copy(temp, buffer, temp.Length);
    }
}