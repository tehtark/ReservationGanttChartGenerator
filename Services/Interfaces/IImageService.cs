using ReservationTimelineGenerator.Models;

namespace ReservationTimelineGenerator.Services.Interfaces;

internal interface IImageService
{
    void GenerateImage(List<Reservation> records);
}