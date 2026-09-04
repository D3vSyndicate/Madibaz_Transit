namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class TripStatusHistory
    {
        public Guid HistoryId { get; set; }

        public Guid TripId { get; set; }

        public Trip Trip { get; set; } = null!;

        public TripStatus TripStatus { get; set; }

        public DateTime Timestamp { get; set; }

        public required string CreatedBy { get; set; }
    }

    public enum TripStatus
    {
        WAITING,
        BOARDING,
        DEPARTED,
        ARRIVED,
        COMPLETED
    }
}