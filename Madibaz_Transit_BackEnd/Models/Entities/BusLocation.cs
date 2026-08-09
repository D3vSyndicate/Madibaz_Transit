namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class BusLocation
    {
        public int Id { get; set; }

        public int BusId { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public decimal Heading { get; set; }

        public string CurrentStatus { get; set; } = "Waiting";

        public DateTime LastUpdated { get; set; }
    }

}
