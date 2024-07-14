using ReservationTimelineGenerator.Models;
using ReservationTimelineGenerator.Services.Interfaces;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Versioning;

namespace ReservationTimelineGenerator.Services;

[SupportedOSPlatform("windows")]
internal class ImageService : IImageService
{
    private const int ChartWidth = 1920;
    private const int ChartHeight = 1080;
    private const int RowHeight = 30;
    private const int BarThickness = 28;
    private const int Margin = 40;
    private const double ReservationTime = 1.75;
    private const int TotalTables = 11;

    private static readonly StringFormat StringFormat = new StringFormat {
        Alignment = StringAlignment.Center,
        LineAlignment = StringAlignment.Near
    };

    private static readonly Pen AxisPen = new Pen(Color.Black, 2);
    private static readonly Pen ReservationPen = new Pen(Color.Gray, 1);
    //private static readonly SolidBrush ReservationBrush = new SolidBrush(Color.LightGray);

    private static readonly SolidBrush ReservationBrush = new SolidBrush(Color.FromArgb(155, 194, 230));
    private static Font ReservationBlockFont = new Font("Verdana", 12, FontStyle.Bold);
    private static Font InformationFont = new Font("Verdana", 14, FontStyle.Bold);
    private static Font ChartFont = new Font("Verdana", 10, FontStyle.Bold);

    public void GenerateImage(List<Reservation> records)
    {
        var date = DateTime.Parse(records[0].Time).Date;
        var firstTable = DateTime.Parse(records[0].Time).Hour - 1;
        var lastTable = DateTime.Parse(records[^1].Time).Hour + 3;

        int dayLength = lastTable - firstTable;
        int timeUnitWidth = (ChartWidth - 2 * Margin) / dayLength;

        using Bitmap bitmap = new Bitmap(ChartWidth, ChartHeight);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);

        DrawAxis(graphics);

        DrawTimeslotMarkings(graphics, firstTable, lastTable, timeUnitWidth);

        DrawReservationBlocks(records, graphics, firstTable, timeUnitWidth);

        DrawInformation(records, graphics);

        SaveImage(bitmap, date);
    }

    private void SaveImage(Bitmap bitmap, DateTime date)
    {
        string fileName = $"Table Reservations Gantt Chart - {date:dd-MM-yyyy}.png";
        string filePath = $"output/{fileName}";

        try {
            bitmap.Save(filePath);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Image exported to: {filePath}");
            Console.ResetColor();
            Process.Start("explorer.exe", $"/open,{AppDomain.CurrentDomain.BaseDirectory}output\\{fileName}");
        }
        catch (Exception error) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();
        }
    }

    private void DrawAxis(Graphics graphics)
    {
        graphics.DrawLine(AxisPen, Margin, Margin + 10, ChartWidth - Margin, Margin + 10);
        graphics.DrawLine(AxisPen, Margin, Margin, Margin, Margin + (TotalTables -1) * RowHeight + Margin);
    }

    private void DrawTimeslotMarkings(Graphics graphics, int firstTable, int lastTable, int timeUnitWidth)
    {
        for (double time = firstTable; time <= lastTable; time += 0.25) {
            int x = Margin + (int)((time - firstTable) * timeUnitWidth);
            int lineHeight = time % 1 == 0 ? 10 : 5;
            graphics.DrawLine(AxisPen, x, Margin, x, Margin + lineHeight);

            if (time % 1 == 0 || time % 1 == 0.5) {
                int hour = (int)time;
                int minute = (int)((time - hour) * 60);

                // Corrected time label calculation
                string timeLabel = $"{((hour == 0 || hour == 12) ? 12 : hour % 12)}:{minute:00} {(hour < 12 ? "AM" : "PM")}";

                graphics.DrawString(timeLabel, ChartFont, Brushes.Black, x, Margin - 25, StringFormat);
            }
        }
    }


    private void DrawReservationBlocks(List<Reservation> records, Graphics graphics, int firstTable, int timeUnitWidth)
    {
        int y = 0;
        for (int t = 0; t < TotalTables; t++) {
            y = Margin + t * RowHeight;
            graphics.DrawString((t + 1).ToString(), ChartFont, Brushes.Black, Margin - 20, y + RowHeight / 2 + 3, StringFormat);
            graphics.DrawLine(ReservationPen, Margin, y + Margin, ChartWidth - Margin, y + Margin);
            foreach (Reservation r in records) {
                if (r.Table == null) throw new NullReferenceException();
                string[] tables = r.Table.Split('+');
                if (tables.Contains((t + 1).ToString())) {
                    if(r.Time == null) throw new NullReferenceException();

                    DateTime startTime = DateTime.Parse(r.Time);
                    DateTime endTime = startTime.AddHours(ReservationTime);

                    int barStartX = Margin + (int)((startTime.Hour - firstTable) * timeUnitWidth + startTime.Minute / 60.0 * timeUnitWidth);
                    int barEndX = Margin + (int)((endTime.Hour - firstTable) * timeUnitWidth + endTime.Minute / 60.0 * timeUnitWidth);
                    int barWidth = Math.Max(barEndX - barStartX, 1);

                    if (barEndX > ChartWidth - Margin) {
                        barWidth = ChartWidth - Margin - barStartX;
                    }

                    graphics.FillRectangle(ReservationBrush, barStartX, y + (RowHeight - BarThickness) / 2 + 10, barWidth, BarThickness);

                    int textX = (barStartX + barEndX) / 2;
                    int textY = y + (RowHeight / 2 + 2);
                    if(r.Name == null) throw new NullReferenceException();
                    
                    string abbreviatedName = AbbreviateName(r.Name);
                    graphics.DrawString($"{abbreviatedName} T:{r.Table} C:{r.Covers}", ReservationBlockFont, Brushes.Black, textX, textY - 1, StringFormat);
                }
            }
        }
    }
    
private void DrawInformation(List<Reservation> records, Graphics graphics)
    {
        int lastTableY = Margin + (TotalTables - 1) * RowHeight; // Calculate the y-coordinate of the last table

        for (var i = 0; i < records.Count; i++) {
            var r = records[i];
            if (r.Name == null) throw new NullReferenceException();

            string information = $"Name: {r.Name}, Phone Number: {r.PhoneNumber}, Email: {r.Email}, Allergies: {r.Allergies}";

            // Calculate the position to draw the information
            int x = Margin;
            int y = lastTableY + Margin + (i + 1) * RowHeight; // Place the strings underneath the last table

            // Draw the information string
            graphics.DrawString(information, InformationFont, Brushes.Black, x, y);
        }
    }
    private string AbbreviateName(string name)
    {
        const int maxLength = 15;
        if (name.Length > maxLength) {
            return name.Substring(0, maxLength - 3) + "...";
        }
        return name;
    }

    
}