namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class Announcements
    {
        public Guid AnnocementId { get; set; }
        public required string Title { get; set; }
        public required string Message { get; set; }
        public required string Priority { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
