namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public enum ShuttleRequestStatus
    {
        Pending,
        Approved,
        Rejected,
        Completed,
        Cancelled
    }

    public class ShuttleRequests
    {
        public int Id { get; set; }

        public int AppUserId { get; set; }

        public AppUser AppUser { get; set; } = null!;

        public string PickupLocation { get; set; } = string.Empty;

        public string Destination { get; set; } = string.Empty;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public ShuttleRequestStatus Status { get; set; }
            = ShuttleRequestStatus.Pending;

        public string? Notes { get; set; }
    }
}
