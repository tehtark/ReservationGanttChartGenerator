using CsvHelper.Configuration.Attributes;

namespace ReservationTimelineGenerator.Models;

public class Reservation
{
    [Name("Time")]
    public string? Time { get; set; }

    [Name("Team note")]
    public string? TeamNote { get; set; }

    [Name("Name")]
    public string? Name { get; set; }

    [Name("Party size")]
    public string? Covers { get; set; }

    [Name("Table name")]
    public string? Table { get; set; }

    [Name("Status")]
    public string? Status { get; set; }

    [Name("Source")]
    public string? Source { get; set; }

    [Name("Creation date")]
    public string? CreationDate { get; set; }

    [Name("Phone number")]
    public string? PhoneNumber { get; set; }

    [Name("Email")]
    public string? Email { get; set; }

    [Name("Any Allergies or Dietary requirements?")]
    public string? Allergies { get; set; }
}