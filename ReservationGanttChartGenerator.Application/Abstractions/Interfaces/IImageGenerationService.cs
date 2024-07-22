using ReservationGanttChartGenerator.Domain.Models;
using SixLabors.ImageSharp;

namespace ReservationGanttChartGenerator.Application.Abstractions.Interfaces;

internal interface IImageGenerationService
{
    string ConvertImageToString(Image image);

    Image GenerateImage(List<Reservation> reservations);
}