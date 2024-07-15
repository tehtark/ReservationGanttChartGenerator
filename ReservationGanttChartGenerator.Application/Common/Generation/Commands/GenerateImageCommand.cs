using MediatR;
using ReservationGanttChartGenerator.Application.Abstractions.Interfaces;
using ReservationGanttChartGenerator.Domain.Models;
using SixLabors.ImageSharp;

namespace ReservationGanttChartGenerator.Application.Common.Generation.Commands;

public record GenerateImageCommand(List<Reservation> Reservations) : IRequest<Image>;

internal class GemerateImageCommandHandler(IImageGenerationService _imageGenerationService) : IRequestHandler<GenerateImageCommand, Image>
{
    public Task<Image> Handle(GenerateImageCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_imageGenerationService.GenerateImage(request.Reservations));
    }
}