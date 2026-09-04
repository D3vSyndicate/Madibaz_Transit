namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class Announcements
    {
        public Guid AnnouncementId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}