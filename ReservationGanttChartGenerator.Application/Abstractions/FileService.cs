using CsvHelper;
using CsvHelper.Configuration;
using ReservationGanttChartGenerator.Application.Abstractions.Interfaces;
using ReservationGanttChartGenerator.Domain.Models;
using System.Globalization;

namespace ReservationGanttChartGenerator.Application.Abstractions;

public class FileService : IFileService
{
    public async Task<List<Reservation>?> ReadFileStream(Stream file)
    {
        if (file == null) return null;
        var records = new List<Reservation>();
        var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture) {
            HasHeaderRecord = true
        };

        using var reader = new StreamReader(file);
        var csv = new CsvReader(reader, csvConfiguration);

        var recordsAsync = csv.GetRecordsAsync<Reservation>();

        await foreach (var record in recordsAsync) {
            records.Add(record);
        }

        csv.Dispose();

        return records;
    }
}