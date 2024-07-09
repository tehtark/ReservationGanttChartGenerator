using CsvHelper;
using CsvHelper.Configuration;
using ReservationTimelineGenerator;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.Versioning;
using static System.Net.Mime.MediaTypeNames;

internal class Program
{
    private static void Main(string[] args)
    {
        Setup();

        string? filePath = string.Empty;

        while (string.IsNullOrEmpty(filePath)) {
            filePath = GetFilePath();
        }

        Console.Clear();

        var records = GetCSVRecords(filePath);

        try {
#pragma warning disable CA1416 // Validate platform compatibility
            CreateImageFromRecords(records);
#pragma warning restore CA1416 // Validate platform compatibility
        }
        catch (Exception) {
            throw;
        }
    }

    private static void Setup()
    {
        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "output")) {
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "output");
        }
        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "input")) {
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "input");
        }
    }

    public static string? GetFilePath()
    {
        string directory = AppDomain.CurrentDomain.BaseDirectory + "input";
        string fileExtension = ".csv";

        string[] files = Directory.GetFiles(directory, "*" + fileExtension);

        int filesLength = files.Length;

        if (filesLength == 0) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No files found! Program will now exit...");
            Console.ResetColor();
            Thread.Sleep(2000);
            Process.GetCurrentProcess().Kill();
        }

        string? fileIndex = null;

        // If there are more than 1 files, display a list of files
        if (filesLength >= 2) {
            for (int i = 0; i < filesLength; i++) {
                Console.WriteLine($"{i + 1} {files[i]}");
            }
            Console.Write("Please select a file: ");
            fileIndex = Console.ReadLine();

            if (fileIndex == null) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Cannot be null");
                Console.ResetColor();
                return null;
            }

            if (!int.TryParse(fileIndex, out int index)) {
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

            fileIndex = files[index - 1];
        }

        if (filesLength == 1) {
            fileIndex = files[0];
        }

        return fileIndex;
    }

    public static List<Reservation> GetCSVRecords(string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
            HasHeaderRecord = false,
        };

        var reader = new StreamReader(filePath);
        var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<Reservation>().ToList();

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

    [SupportedOSPlatform("windows")]
    public static void CreateImageFromRecords(List<Reservation> records)
    {
        // Gantt Chart Configuration
        int chartWidth = 1920;
        int chartHeight = 1080;
        int rowHeight = 30;
        int margin = 40;
        int dayLength = 24;

        // Create Bitmap
        Bitmap bitmap = new(chartWidth, chartHeight);
        Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);

        // Draw Timeline Axis (X Axis)
        Pen xPen = new Pen(Color.Black, 2);
        graphics.DrawLine(xPen, margin, margin + 10, margin, chartHeight - margin);
        for (int i = 0; i <= dayLength; i++) {
            int x = margin + (i * chartWidth / dayLength);
            graphics.DrawLine(xPen, x, margin, x, margin + 5); // Draw line under number
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Near;
            graphics.DrawString(i.ToString(), SystemFonts.DefaultFont, Brushes.Black, x, margin - 15, stringFormat); // Draw number
        }

        // Draw Table line Axies (Y Axis)
        Pen yPen = new Pen(Color.Gray, 1);
        Brush reservationBrush = Brushes.LightBlue;
        System.Drawing.Font font = SystemFonts.DefaultFont;
        graphics.DrawLine(xPen, margin, margin + 10, chartWidth - margin, margin + 10);
        for (int i = 0; i < records.Count; i++) {
            Reservation r = records[i];
            int y = margin + (i * rowHeight);

            // Draw TableName on the LEFT before the line
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Near;
            stringFormat.LineAlignment = StringAlignment.Center;
            graphics.DrawString(r.Table, font, Brushes.Black, margin - 25, y + (rowHeight / 2) + 10, stringFormat);
            graphics.DrawLine(yPen, margin, y + margin, chartWidth - margin, y + margin); // Add some padding after the table name

            // Parse Time
            DateTime startTime = DateTime.Parse(records[i].Time);
            DateTime endTime = startTime.AddHours(2); // Estimate 2-hour reservation (adjust if needed)

            int barStartX = margin + (int)((startTime.Hour + startTime.Minute / 60.0) * (chartWidth / dayLength));
            int barEndX = margin + (int)((endTime.Hour + endTime.Minute / 60.0) * (chartWidth / dayLength));
            int barWidth = barEndX - barStartX;
            graphics.FillRectangle(reservationBrush, barStartX, y + 20, barWidth, rowHeight - 20);
        }

        // Save Bitmap
        bitmap.Save($"output/table_reservations_gantt.png");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Gantt chart created: output/table_reservations_gantt.png");
        Console.ResetColor();
    }
}