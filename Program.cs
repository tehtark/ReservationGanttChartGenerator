using ReservationTimelineGenerator.Services;
using ReservationTimelineGenerator.Services.Interfaces;
using System.Runtime.Versioning;

[SupportedOSPlatform("windows")]
internal class Program
{
    public static IFileService FileService = new FileService();
    public static IImageService GenerationService = new ImageService();

    private static void Main(string[] args)
    {
        Setup();

        string? filePath = string.Empty;

        while (string.IsNullOrEmpty(filePath)) {
            filePath = FileService.GetFilePath();
        }

        Console.Clear();

        var records = FileService.ReadFile(filePath);

        try {
            GenerationService.GenerateImage(records);
        }
        catch (Exception) {
            throw;
        }
    }

    private static void Setup()
    {
        if (!FileService.OutputDirectoryCheck()) {
            FileService.CreateDirectory("output");
        };

        if (!FileService.InputDirectoryCheck()) {
            FileService.CreateDirectory("input");
        };
    }
}