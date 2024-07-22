using MediatR;
using ReservationGanttChartGenerator.Application.Abstractions.Interfaces;
using SixLabors.ImageSharp;

namespace ReservationGanttChartGenerator.Application.Common.Generation.Commands;
public record ConvertImageToStringCommand(Image image) : IRequest<string>;

internal class ConvertImageToStringCommandHandler(IImageGenerationService ImageGenerationService) : IRequestHandler<ConvertImageToStringCommand, string>
{
    public Task<string> Handle(ConvertImageToStringCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(ImageGenerationService.ConvertImageToString(request.image));
    }
}