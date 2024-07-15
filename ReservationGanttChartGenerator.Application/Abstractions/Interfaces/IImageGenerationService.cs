using ReservationGanttChartGenerator.Domain.Models;
using SixLabors.ImageSharp;

namespace ReservationGanttChartGenerator.Application.Abstractions.Interfaces;

internal interface IImageGenerationService
{
    public Image GenerateImage(List<Reservation> reservations);
}