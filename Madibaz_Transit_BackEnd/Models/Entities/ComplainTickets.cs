namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public enum ComplaintStatus
    {
        Open,
        InReview,
        Resolved,
        Dismissed
    }

    public class ComplainTickets
    {
        public int Id { get; set; }

        public int AppUserId { get; set; }

        public AppUser AppUser { get; set; } = null!;

        public Guid? TripId { get; set; }

        public Trip? Trip { get; set; }

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public ComplaintStatus Status { get; set; }
            = ComplaintStatus.Open;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }
    }
}