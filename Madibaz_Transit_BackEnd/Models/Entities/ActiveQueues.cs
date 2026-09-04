namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public enum QueueEntryStatus
    {
        Waiting,
        Promoted,
        Expired,
        Cancelled
    }

    public class ActiveQueues
    {
        public Guid Id { get; set; }

        public int AppUserId { get; set; }

        public AppUser AppUser { get; set; } = null!;

        public Guid TripId { get; set; }

        public Trip Trip { get; set; } = null!;

        public int Position { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public QueueEntryStatus Status { get; set; }
            = QueueEntryStatus.Waiting;
    }
}