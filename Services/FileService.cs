using CsvHelper;
using CsvHelper.Configuration;
using ReservationTimelineGenerator.Models;
using ReservationTimelineGenerator.Services.Interfaces;
using System.Diagnostics;
using System.Globalization;

namespace ReservationTimelineGenerator.Services;

internal class FileService : IFileService
{
    public bool OutputDirectoryCheck()
    {
        return Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "output") ? true : false;
    }

    public bool InputDirectoryCheck()
    {
        return Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "input") ? true : false;
    }

    public bool CreateDirectory(string directory)
    {
        var directoryInfo = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + directory);

        if (directoryInfo == null) return false; else return true;
    }

    public string[] GetFiles()
    {
        string directory = AppDomain.CurrentDomain.BaseDirectory + "input";
        string fileExtension = ".csv";

        return Directory.GetFiles(directory, "*" + fileExtension);
    }

    public string? GetFilePath()
    {
        string[] files = GetFiles();

        int filesLength = files.Length;

        if (filesLength == 0) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No files found! Program will now exit...");
            Console.ResetColor();
            Thread.Sleep(2000);
            Process.GetCurrentProcess().Kill();
        }

        string? path = null;

        // If there are more than 1 files, display a list of files
        if (filesLength >= 2) {
            for (int i = 0; i < filesLength; i++) {
                Console.WriteLine($"{i + 1} {files[i]}");
            }
            Console.Write("Please select a file: ");
            path = Console.ReadLine();

            if (path == null) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Cannot be null");
                Console.ResetColor();
                return null;
            }

            if (!int.TryParse(path, out int index)) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid input, must be a number");
                Console.ResetColor();
                return null;
            }

            if (index < 1 || index > filesLength) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid input, must be a valid file index");
                Console.ResetColor();
                return null;
            }

            path = files[index - 1];
        }

        if (filesLength == 1) {
            path = files[0];
        }

        return path;
    }

    public List<Reservation> ReadFile(string path)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
            HasHeaderRecord = false,
        };

        var reader = new StreamReader(path);
        var csv = new CsvReader(reader, config);
        List<Reservation>? records = csv.GetRecords<Reservation>().ToList();
        reader.Dispose();

        if (records == null) {
            Console.WriteLine("Cannot read file! Program will now exit...");
            Console.ResetColor();
            Thread.Sleep(2000);
            Process.GetCurrentProcess().Kill();
        }

        for (int i = records.Count - 1; i >= 0; i--) {
            if (records[i].Name.ToLower() == "euro projector") {
                records.RemoveAt(i);
            }
        }

        records.RemoveAt(0);

        foreach (var record in records) {
            DateTime dateTime = DateTime.Parse(record.Time);
            TimeOnly time = TimeOnly.FromDateTime(dateTime);

            //Console.WriteLine($"Time: {record.Time}");
            Console.WriteLine($"Time: {time}");
            Console.WriteLine($"Name: {record.Name}");
            Console.WriteLine($"Covers: {record.Covers}");
            Console.WriteLine($"Table: {record.Table}");
            Console.WriteLine($"Phone Number: {record.PhoneNumber}");

            if (!string.IsNullOrEmpty(record.Allergies)) {
                Console.WriteLine($"Allergies: {record.Allergies}");
            }

            Console.WriteLine();
        }

        return records;
    }
}