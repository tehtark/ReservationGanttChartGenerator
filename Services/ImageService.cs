using ReservationTimelineGenerator.Models;
using ReservationTimelineGenerator.Services.Interfaces;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Versioning;

namespace ReservationTimelineGenerator.Services;

/// <summary>
/// Service class for generating table reservation Gantt chart images.
/// </summary>
[SupportedOSPlatform("windows")]
internal class ImageService : IImageService
{
    /// <summary>
    /// The width of the chart image.
    /// </summary>
    private const int ChartWidth = 1920;

    /// <summary>
    /// The height of the chart image.
    /// </summary>
    private const int ChartHeight = 1080;

    /// <summary>
    /// The height of each row in the chart.
    /// </summary>
    private const int RowHeight = 30;

    /// <summary>
    /// The thickness of the reservation bars in the chart.
    /// </summary>
    private const int BarThickness = 28;

    /// <summary>
    /// The margin around the chart.
    /// </summary>
    private const int Margin = 40;

    /// <summary>
    /// The duration of each reservation time slot.
    /// </summary>
    private const double ReservationTime = 1.75;

    /// <summary>
    /// The total number of tables in the chart.
    /// </summary>
    private const int TotalTables = 11;

    /// <summary>
    /// The string format used for aligning text in the chart.
    /// </summary>
    private static readonly StringFormat StringFormat = new StringFormat {
        Alignment = StringAlignment.Center,
        LineAlignment = StringAlignment.Near
    };

    /// <summary>
    /// The pen used for drawing the chart axis.
    /// </summary>
    private static readonly Pen AxisPen = new Pen(Color.Black, 2);

    /// <summary>
    /// The pen used for drawing the reservation bars.
    /// </summary>
    private static readonly Pen ReservationPen = new Pen(Color.Gray, 1);

    /// <summary>
    /// The brush used for filling the reservation bars.
    /// </summary>
    private static readonly SolidBrush ReservationBrush = new SolidBrush(Color.FromArgb(155, 194, 230));

    /// <summary>
    /// The font used for the reservation block text.
    /// </summary>
    private static Font ReservationBlockFont = new Font("Verdana", 11, FontStyle.Bold);

    /// <summary>
    /// The font used for the information text.
    /// </summary>
    private static Font InformationFont = new Font("Verdana", 12, FontStyle.Regular);

    /// <summary>
    /// The font used for the chart labels.
    /// </summary>
    private static Font ChartFont = new Font("Verdana", 10, FontStyle.Bold);

    /// <summary>
    /// Generates a table reservation Gantt chart image based on the provided reservation records.
    /// </summary>
    /// <param name="records">The list of reservation records.</param>
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

    /// <summary>
    /// Saves the generated chart image to a file.
    /// </summary>
    /// <param name="bitmap">The chart image bitmap.</param>
    /// <param name="date">The date of the reservations.</param>
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

    /// <summary>
    /// Draws the chart axis.
    /// </summary>
    /// <param name="graphics">The graphics object used for drawing.</param>
    private void DrawAxis(Graphics graphics)
    {
        graphics.DrawLine(AxisPen, Margin, Margin + 10, ChartWidth - Margin, Margin + 10);
        graphics.DrawLine(AxisPen, Margin, Margin, Margin, Margin + (TotalTables - 1) * RowHeight + Margin);
    }

    /// <summary>
    /// Draws the time slot markings on the chart.
    /// </summary>
    /// <param name="graphics">The graphics object used for drawing.</param>
    /// <param name="firstTable">The index of the first table in the chart.</param>
    /// <param name="lastTable">The index of the last table in the chart.</param>
    /// <param name="timeUnitWidth">The width of each time unit in pixels.</param>
    private void DrawTimeslotMarkings(Graphics graphics, int firstTable, int lastTable, int timeUnitWidth)
    {
        for (double time = firstTable; time <= lastTable; time += 0.25) {
            int x = Margin + (int)((time - firstTable) * timeUnitWidth);
            int lineHeight = time % 1 == 0 ? 10 : 5;
            graphics.DrawLine(AxisPen, x, Margin, x, Margin + lineHeight);

            if (time % 1 == 0 || time % 1 == 0.5) {
                int hour = (int)time;
                int minute = (int)((time - hour) * 60);

                if (hour == 24) {
                    hour = 0;
                }

                string timeLabel = $"{hour:00}:{minute:00}";

                graphics.DrawString(timeLabel, ChartFont, Brushes.Black, x, Margin - 25, StringFormat);
            }
        }
    }

    /// <summary>
    /// Draws the reservation blocks on the chart.
    /// </summary>
    /// <param name="records">The list of reservation records.</param>
    /// <param name="graphics">The graphics object used for drawing.</param>
    /// <param name="firstTable">The index of the first table in the chart.</param>
    /// <param name="timeUnitWidth">The width of each time unit in pixels.</param>
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
                    if (r.Time == null) throw new NullReferenceException();

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
                    if (r.Name == null) throw new NullReferenceException();

                    string abbreviatedName = AbbreviateName(r.Name);
                    graphics.DrawString($"{abbreviatedName} T:{r.Table} C:{r.Covers}", ReservationBlockFont, Brushes.Black, textX, textY - 1, StringFormat);
                }
            }
        }
    }

    /// <summary>
    /// Draws the information strings below the chart.
    /// </summary>
    /// <param name="records">The list of reservation records.</param>
    /// <param name="graphics">The graphics object used for drawing.</param>
    private void DrawInformation(List<Reservation> records, Graphics graphics)
    {
        int lastTableY = Margin + (TotalTables - 1) * RowHeight;

        for (var i = 0; i < records.Count; i++) {
            var r = records[i];
            if (r.Name == null) throw new NullReferenceException();

            string information = $"Name: {AbbreviateName(r.Name, 30)} | Phone Number: {r.PhoneNumber}";
            if (string.IsNullOrEmpty(r.Allergies)) {
                information += $" | Allergies: No";
            }
            else {
                information += $" | Allergies: {r.Allergies}";
            }

            int x = Margin;
            float y = lastTableY + Margin + (i + 1) * RowHeight / 1.5f;

            graphics.DrawString(information, InformationFont, Brushes.Black, x, y);
        }
    }

    /// <summary>
    /// Abbreviates the given name if it exceeds the maximum length.
    /// </summary>
    /// <param name="name">The name to abbreviate.</param>
    /// <param name="maxLength">The maximum length of the abbreviated name.</param>
    /// <returns>The abbreviated name.</returns>
    private string AbbreviateName(string name, int maxLength = 15)
    {
        if (name.Length > maxLength) {
            return name.Substring(0, maxLength - 3) + "...";
        }
        return name;
    }
}