using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using ReservationGanttChartGenerator.Application.Abstractions.Interfaces;
using ReservationGanttChartGenerator.Domain.Models;

namespace ReservationGanttChartGenerator.Application.Common.File.Queries;
public record ReadFileQuery(Stream File) : IRequest<List<Reservation>?>;

internal class ReadFileStreamQueryHandler(IFileService _fileService) : IRequestHandler<ReadFileQuery, List<Reservation>?>
{
    public async Task<List<Reservation>?> Handle(ReadFileQuery request, CancellationToken cancellationToken)
    {
        return await _fileService.ReadFileStream(request.File);
    }
}