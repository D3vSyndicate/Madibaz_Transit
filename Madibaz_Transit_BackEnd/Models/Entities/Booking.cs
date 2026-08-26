namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class Booking
    {
        public int Id { get; set; }

        public int AppUserId { get; set; }

        public int ScheduledTripId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Confirmed";

        public string? BoardingToken { get; set; }

        // Records whether the student confirmed that they are
        // still attending the trip during the 30-minute confirmation window.
        public bool AttendanceConfirmed { get; set; } = false;

        public AppUser AppUser { get; set; } = null!;

        public ScheduledTrip ScheduledTrip { get; set; } = null!;
    }
}