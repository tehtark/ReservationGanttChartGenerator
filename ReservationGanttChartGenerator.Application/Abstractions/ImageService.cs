using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ReservationGanttChartGenerator.Application.Abstractions;

internal class ImageService
{
    public Image GenerateImage()
    {
        Image<Rgba32> image = new(1920, 1080);

        SaveImage(image, AppDomain.CurrentDomain.BaseDirectory + "Temp/");
        return image;
    }

    public bool SaveImage(Image image, string path)
    {
        bool result = false;

        return result;
    }
}