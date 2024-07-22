using ReservationGanttChartGenerator.Application.Abstractions.Interfaces;
using ReservationGanttChartGenerator.Domain.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ReservationGanttChartGenerator.Application.Abstractions;

internal class ImageGenerationService : IImageGenerationService
{
    private int _width = 1920;
    private int _height = 1080;
    private int _margin = 30;
    private int _rowHeight = 30;
    private int _totalTables = 11;
    private double _reservationDuration = 1.75f;
    private int _barThickness = 26;

    private Font _font = SystemFonts.CreateFont("Arial", 16);

    private DrawingOptions _drawingOptions = new DrawingOptions() {
        GraphicsOptions = new GraphicsOptions() {
            Antialias = true, // Smoother lines
            BlendPercentage = 1f // Adjust line opacity (0.0 - 1.0)
        }
    };

    private SolidPen _pen = Pens.Solid(Color.Black, 2);

    public Image GenerateImage(List<Reservation> reservations)
    {
        Image image = new Image<Rgba32>(_width, _height);
        image.Mutate(x => x.BackgroundColor(Color.White));

        var firstTable = DateTime.Parse(reservations.First().Time).Hour - 1;
        var lastTable = DateTime.Parse(reservations.Last().Time).Hour + 3;

        int dayLength = lastTable - firstTable;
        int timeUnitWidth = (_width - 2 * _margin) / dayLength;

        DrawAxis(image);
        DrawTimeScale(image, firstTable, lastTable, timeUnitWidth);
        DrawReservations(image, reservations, firstTable, lastTable, timeUnitWidth);
        DrawInformation(image, reservations);

        return image;
    }

    public string ConvertImageToString(Image image)
    {
        using var stream = new MemoryStream();
        image.Save(stream, new PngEncoder());
        byte[] imageBytes = stream.ToArray();

        string base64Image = Convert.ToBase64String(imageBytes);
        var dataUri = $"data:image/png;base64,{base64Image}";
        return dataUri;
    }

    private void DrawAxis(Image image)
    {
        image.Mutate(img => img.DrawLine(_drawingOptions, _pen, new Point(_margin, _margin), new Point(_width - _margin, _margin)));
        image.Mutate(img => img.DrawLine(_drawingOptions, _pen, new Point(_margin, _margin), new Point(_margin, _margin + (_totalTables) * _rowHeight))); // Updated line
    }

    private void DrawTimeScale(Image image, int firstTable, int lastTable, int timeUnitWidth)
    {
        for (double time = firstTable; time <= lastTable; time += 0.25) {
            int x = _margin + (int)((time - firstTable) * timeUnitWidth);
            image.Mutate(img => img.DrawLine(_drawingOptions, _pen, new Point(x, _margin), new Point(x, _margin - 10)));

            if (time % 1 == 0 || time % 1 == 0.5) {
                int hour = (int)time;
                int minute = (int)((time - hour) * 60);

                if (hour == 24) {
                    hour = 0;
                }

                string timeLabel = $"{hour:00}:{minute:00}";

                image.Mutate(img => img.DrawText(timeLabel, _font, Color.Black, new PointF(x - 20, _margin - 28)));
            }
        }
    }

    private void DrawReservations(Image image, List<Reservation> records, int firstTable, int lastTable, int timeUnitWidth)
    {
        int y = 0;
        for (int t = 0; t < _totalTables; t++) {
            y = _margin + t * _rowHeight;
            int textX = _margin - 16;
            int textY = y + _rowHeight / 2 - 2;
            string tableNumber = (t + 1).ToString();
            var textSize = TextMeasurer.MeasureSize(tableNumber, new TextOptions(_font));
            var textLocation = new PointF(textX - textSize.Width / 2, textY - textSize.Height / 2);
            image.Mutate(img => img.DrawText(tableNumber, _font, Color.Black, textLocation));
            image.Mutate(img => img.DrawLine(_drawingOptions, _pen, new Point(_margin, y + _margin), new Point(_width - _margin, y + _margin)));

            var tableReservations = records.Where(r => r.Table != null && r.Table.Split('+').Contains((t + 1).ToString())).ToList();
            foreach (Reservation r in tableReservations) {
                if (r.Time == null) throw new NullReferenceException();

                DateTime startTime = DateTime.Parse(r.Time);
                DateTime endTime = startTime.AddHours(_reservationDuration);

                int barStartX = _margin + (int)((startTime.Hour - firstTable) * timeUnitWidth + startTime.Minute / 60.0 * timeUnitWidth);
                int barEndX = _margin + (int)((endTime.Hour - firstTable) * timeUnitWidth + endTime.Minute / 60.0 * timeUnitWidth);
                int barWidth = Math.Max(barEndX - barStartX, 1);

                if (barEndX > _width - _margin) {
                    barWidth = _width - _margin - barStartX;
                }

                bool isOverlapping = tableReservations.Any(other =>
                    other != r &&
                    DateTime.Parse(other.Time).AddHours(_reservationDuration) == DateTime.Parse(r.Time)
                );

                var fillColor = isOverlapping ? Color.LightSalmon : Color.LightBlue;

                var rect = new Rectangle(barStartX, y + (_rowHeight - _barThickness) / 2, barWidth, _barThickness);
                image.Mutate(img => img.Fill(fillColor, rect));

                // Centering Text Logic
                textX = rect.Left + rect.Width / 2;         // Center text horizontally within the rectangle
                textY = rect.Top + rect.Height / 2;         // Center text vertically within the rectangle

                if (r.Name == null) throw new NullReferenceException();

                string textToDisplay = $"{AbbreviateName(r.Name)} T: {r.Table} C:{r.Covers}";
                textSize = TextMeasurer.MeasureSize(textToDisplay, new TextOptions(_font));
                textLocation = new PointF(textX - textSize.Width / 2, textY - textSize.Height / 2); // Adjust text position

                image.Mutate(img => img.DrawText(textToDisplay, _font, Color.Black, textLocation));
            }
        }
    }

    private void DrawInformation(Image image, List<Reservation> records)
    {
        int lastTableY = _margin + (_totalTables - 1) * _rowHeight;

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

            int x = _margin;
            float y = lastTableY + _margin + (i + 1) * _rowHeight / 1.5f;

            image.Mutate(img => img.DrawText(information, _font, Color.Black, new PointF(x, y)));
        }
    }

    private string AbbreviateName(string name, int maxLength = 15)
    {
        if (name.Length > maxLength) {
            return name.Substring(0, maxLength - 3) + "...";
        }
        return name;
    }
}