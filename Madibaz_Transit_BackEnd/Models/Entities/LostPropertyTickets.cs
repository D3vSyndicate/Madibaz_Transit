namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public enum LostPropertyStatus
    {
        Reported,
        Found,
        Collected,
        Closed
    }

    public class LostPropertyTickets
    {
        public int Id { get; set; }

        public int AppUserId { get; set; }

        public AppUser AppUser { get; set; } = null!;

        public Guid? TripId { get; set; }

        public Trip? Trip { get; set; }

        public string ItemDescription { get; set; } = string.Empty;

        public string? Location { get; set; }

        public DateTime DateReported { get; set; } = DateTime.UtcNow;

        public LostPropertyStatus Status { get; set; }
            = LostPropertyStatus.Reported;

        public DateTime? ResolvedAt { get; set; }
    }
}
