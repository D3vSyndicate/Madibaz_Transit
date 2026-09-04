namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class GPSCoordinateHistory
    {
        public int Id { get; set; }

        public int BusId { get; set; }
        public Bus Bus { get; set; } = null!;

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}