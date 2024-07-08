using CsvHelper;
using CsvHelper.Configuration;
using ReservationTimelineGenerator;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;

internal class Program
{
    private static int imageWidth = 800;
    private static int imageHeight = 600;

    private static void Main(string[] args)
    {
        string? filePath = string.Empty;

        while (string.IsNullOrEmpty(filePath)) {
            filePath = GetFilePath();
        }

        Console.Clear();

        var records = GetCSVRecords(filePath);

        try {
            CreateImageFromRecords(records);
        }
        catch (Exception) {
            throw;
        }
    }

    public static string? GetFilePath()
    {
        string directory = AppDomain.CurrentDomain.BaseDirectory.ToString();
        string fileExtension = ".csv";

        string[] files = Directory.GetFiles(directory, "*" + fileExtension);

        int filesLength = files.Length;

        if (filesLength == 0) {
            Console.WriteLine("No .csv files found!");
            return null;
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
                Console.WriteLine("Cannot be null");
                return null;
            }

            if (!int.TryParse(fileIndex, out int index)) {
                Console.WriteLine("Invalid input, must be a number");
                return null;
            }

            if (index < 1 || index > filesLength) {
                Console.WriteLine("Invalid input, must be a valid file index");
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

    public static void CreateImageFromRecords(List<Reservation> records)
    {
        // Gantt Chart Configuration
        int chartWidth = 1920;
        int chartHeight = 1080;
        int rowHeight = 30;
        int margin = 20;

        // Create Bitmap
        Bitmap bitmap = new Bitmap(chartWidth, chartHeight);
        Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);

        // Draw Timeline Axis
        Pen axisPen = new Pen(Color.Black, 2);
        graphics.DrawLine(axisPen, margin, margin, margin, chartHeight - margin);
        for (int i = 0; i <= 24; i++) // Assuming 24-hour timeline
        {
            int x = margin + (i * chartWidth / 24);
            graphics.DrawLine(axisPen, x, margin, x, margin + 5);
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Near;

            if (i == 0 || i == 24) // Special case for 0 and 24 to place them outside the line
            {
                graphics.DrawString(i.ToString(), SystemFonts.DefaultFont, Brushes.Black, x, margin + 10, stringFormat);
            }
            else {
                if (x - 10 >= margin) // Check if there's space to draw the label on the left
                {
                    graphics.DrawString(i.ToString(), SystemFonts.DefaultFont, Brushes.Black, x - 10, margin - 15);
                }
                else // Draw on the right if not enough space on the left
                {
                    graphics.DrawString(i.ToString(), SystemFonts.DefaultFont, Brushes.Black, x + 10, margin - 15);
                }
            }
        }

        // Draw Table Rows and Reservation Bars

        Pen tablePen = new Pen(Color.Gray, 1);
        Brush reservationBrush = Brushes.LightBlue;
        System.Drawing.Font font = SystemFonts.DefaultFont;

        for (int i = 0; i < records.Count; i++) {
            Reservation r = records[i];
            int y = margin + (i * rowHeight);
            graphics.DrawLine(tablePen, margin, y, chartWidth - margin, y);
            graphics.DrawString(r.Table, font, Brushes.Black, margin, y + 5);

            // Parse Time
            DateTime startTime = DateTime.Parse(records[i].Time);
            DateTime endTime = startTime.AddHours(2); // Estimate 2-hour reservation (adjust if needed)

            int barStartX = margin + (int)((startTime.Hour + startTime.Minute / 60.0) * (chartWidth / 24));
            int barEndX = margin + (int)((endTime.Hour + endTime.Minute / 60.0) * (chartWidth / 24));
            int barWidth = barEndX - barStartX;
            graphics.FillRectangle(reservationBrush, barStartX, y + 10, barWidth, rowHeight - 20);
        }

        // Save Bitmap
        bitmap.Save("table_reservations_gantt.png", ImageFormat.Png);
        Console.WriteLine("Gantt chart created: table_reservations_gantt.png");
    }
}