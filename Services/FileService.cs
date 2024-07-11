using CsvHelper;
using CsvHelper.Configuration;
using ReservationTimelineGenerator.Models;
using ReservationTimelineGenerator.Services.Interfaces;
using Spectre.Console;
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
            List<string> fileNames = [];

            foreach (var file in files) {
                fileNames.Add(Path.GetFileName(file));
            }
            path = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Please select a [green]file[/]?")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more files)[/]")
                    .AddChoices(fileNames));

            path = AppContext.BaseDirectory + "input/" + path;
        }

        // If there is only 1 file, set the path to the file
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

        return records;
    }
}