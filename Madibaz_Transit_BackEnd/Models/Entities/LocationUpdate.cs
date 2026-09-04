namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class LocationUpdate
    {
        public Guid LocationId { get; set; }
        public Guid TripId { get; set; }
        public required string Longitude { get; set; }
        public required string Latitude { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
