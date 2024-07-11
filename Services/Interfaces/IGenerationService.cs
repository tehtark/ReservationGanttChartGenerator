using ReservationTimelineGenerator.Models;

namespace ReservationTimelineGenerator.Services.Interfaces;

internal interface IGenerationService
{
    void GenerateImage(List<Reservation> records);
}