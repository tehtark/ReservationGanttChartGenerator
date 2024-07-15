using ReservationGanttChartGenerator.Application.Abstractions.Interfaces;
using ReservationGanttChartGenerator.Domain.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
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

        return image;
    }

    private void DrawAxis(Image image)
    {
        image.Mutate(img => img.DrawLine(_drawingOptions, _pen, new Point(_margin, _margin), new Point(_width - _margin, _margin)));
        image.Mutate(img => img.DrawLine(_drawingOptions, _pen, new Point(_margin, _margin), new Point(_margin, _height - _margin)));
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
            foreach (Reservation r in records) {
                if (r.Table == null) throw new NullReferenceException();
                string[] tables = r.Table.Split('+');
                if (tables.Contains((t + 1).ToString())) {
                    if (r.Time == null) throw new NullReferenceException();

                    DateTime startTime = DateTime.Parse(r.Time);
                    DateTime endTime = startTime.AddHours(_reservationDuration);

                    int barStartX = _margin + (int)((startTime.Hour - firstTable) * timeUnitWidth + startTime.Minute / 60.0 * timeUnitWidth);
                    int barEndX = _margin + (int)((endTime.Hour - firstTable) * timeUnitWidth + endTime.Minute / 60.0 * timeUnitWidth);
                    int barWidth = Math.Max(barEndX - barStartX, 1);

                    if (barEndX > _width - _margin) {
                        barWidth = _width - _margin - barStartX;
                    }

                    var rect = new Rectangle(barStartX, y + (_rowHeight - _barThickness) / 2, barWidth, _barThickness);
                    image.Mutate(img => img.Fill(Color.LightBlue, rect));

                    textX = (barStartX + barEndX) / 2;
                    textY = y + _rowHeight / 2 + 2;
                    if (r.Name == null) throw new NullReferenceException();

                    string abbreviatedName = AbbreviateName(r.Name);
                    textSize = TextMeasurer.MeasureSize(abbreviatedName, new TextOptions(_font));
                    textLocation = new PointF(textX - textSize.Width / 2, textY - textSize.Height / 2 - 3);
                    image.Mutate(img => img.DrawText(abbreviatedName, _font, Color.Black, textLocation));
                }
            }
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