using ReservationTimelineGenerator.Models;

namespace ReservationTimelineGenerator.Services.Interfaces;

internal interface IFileService
{
    bool CreateDirectory(string directory);

    string? GetFilePath();

    string[] GetFiles();

    bool InputDirectoryCheck();

    bool OutputDirectoryCheck();

    List<Reservation> ReadFile(string path);
}