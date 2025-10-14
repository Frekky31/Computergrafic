using StbImageSharp;
using System.IO;

public static class HDRLoader
{
    public static (float[] data, int width, int height) LoadEXR(string path)
    {
        using var stream = File.OpenRead(path);
        var image = ImageResultFloat.FromStream(stream, ColorComponents.RedGreenBlue);
        return (image.Data, image.Width, image.Height);
    }
}