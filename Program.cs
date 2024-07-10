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
        reader.Dispose();

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

    [SupportedOSPlatform("windows")]
    public static void CreateImageFromRecords(List<Reservation> records)
    {
        // Gantt Chart Configuration
        int chartWidth = 1920;
        int chartHeight = 1080;
        int rowHeight = 30;
        int barThickness = 28;
        int margin = 40;
        int reservationTime = 2;

        var date = DateTime.Parse(records[0].Time).Date;
        var firstTable = DateTime.Parse(records.First().Time).Hour - 1;
        var lastTable = DateTime.Parse(records.Last().Time).Hour + 2;

        int dayLength = lastTable - firstTable;
        // Calculate time unit width dynamically to fit the chart
        int timeUnitWidth = (chartWidth - 2 * margin) / dayLength;

        StringFormat stringFormat = new StringFormat();
        stringFormat.Alignment = StringAlignment.Center;
        stringFormat.LineAlignment = StringAlignment.Near;

        Pen axisPen = new Pen(Color.Black, 2);
        Pen reservationPen = new Pen(Color.Gray, 1);
        Brush reservationBrush = Brushes.LightBlue;
        System.Drawing.Font font = SystemFonts.DefaultFont;

        // Create Bitmap
        Bitmap bitmap = new(chartWidth, chartHeight);
        Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);

        graphics.DrawLine(axisPen, margin, margin + 10, chartWidth - margin, margin + 10);
        // Draw timeslot markings (every 15 minutes)
        for (double time = firstTable; time <= lastTable; time += 0.25) // 0.25 represents 15 minutes
        {
            int x = margin + (int)((time - firstTable) * timeUnitWidth);
            int lineHeight = (time % 1 == 0) ? 10 : 5;
            graphics.DrawLine(axisPen, x, margin, x, margin + lineHeight);
            graphics.DrawLine(axisPen, x, margin, x, margin + 5); // Draw line under number

            // Draw time label at the top (only for hours and half-hours)
            if (time % 1 == 0 || time % 1 == 0.5) {
                int hour = (int)time;
                int minute = (int)((time - hour) * 60); // Calculate minutes from fractional part
                string timeLabel = $"{hour % 12}:{minute:00} {(hour < 12 ? "AM" : "PM")}"; // Corrected format
                graphics.DrawString(timeLabel, SystemFonts.DefaultFont, Brushes.Black, x, margin - 25, stringFormat);
            }
        }

        // Draw Table line Axies (Y Axis)
        graphics.DrawLine(axisPen, margin, margin + 10, chartWidth - margin, margin + 10);

        // Reservation drawing and timeslot markings
        for (int i = 0; i < records.Count; i++) {
            Reservation r = records[i];
            int y = margin + (i * rowHeight);
            graphics.DrawString(r.Table, font, Brushes.Black, margin - 25, y + (rowHeight / 2) + 10, new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
            graphics.DrawLine(reservationPen, margin, y + margin, chartWidth - margin, y + margin);

            // Parse Time
            DateTime startTime = DateTime.Parse(r.Time);
            DateTime endTime = startTime.AddHours(reservationTime);

            // Calculate start and end X positions
            int barStartX = margin + (int)((startTime.Hour - firstTable) * timeUnitWidth + (startTime.Minute / 60.0) * timeUnitWidth);
            int barEndX = margin + (int)((endTime.Hour - firstTable) * timeUnitWidth + (endTime.Minute / 60.0) * timeUnitWidth);
            int barWidth = Math.Max(barEndX - barStartX, 1); // Ensure minimum width of 1 pixel

            // Check if reservation extends beyond the chart
            if (barEndX > chartWidth - margin) {
                barWidth = chartWidth - margin - barStartX; // Limit width to fit the chart
            }

            // Draw reservation block with adjusted positions
            graphics.FillRectangle(reservationBrush, barStartX, y + ((rowHeight - barThickness) / 2 + 10), barWidth, barThickness);
        }

        // Save Bitmap
        bitmap.Save($"output/Table Reservations Gantt Chart - {date.ToString("yyyy-MM-dd")}.png");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Image created at: output/Table Reservations Gantt Chart - {date.ToString("yyyy-MM-dd")}.png");
        Console.ResetColor();
    }
}