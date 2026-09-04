namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public enum NotificationType { BookingConfirmation, QueueUpdate, BoardingReminder, TripStatusUpdate, ServiceAnnouncement }

    public class Notifications
    {
        public int Id { get; set; }
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; } = null!;

        public NotificationType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
    }
}
