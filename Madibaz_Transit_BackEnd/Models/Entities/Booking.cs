namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class Booking
    {
        public int Id { get; set; }

        public int AppUserId { get; set; }

        public int ScheduledTripId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Reserved";

        public string? BoardingToken { get; set; }

        public AppUser AppUser { get; set; } = null!;

        public ScheduledTrip ScheduledTrip { get; set; } = null!;
    }
}