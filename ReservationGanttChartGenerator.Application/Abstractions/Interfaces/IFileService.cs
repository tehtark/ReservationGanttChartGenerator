using ReservationGanttChartGenerator.Domain.Models;

namespace ReservationGanttChartGenerator.Application.Abstractions.Interfaces;

public interface IFileService
{
    public Task<List<Reservation>?> ReadFileStream(Stream file);
}