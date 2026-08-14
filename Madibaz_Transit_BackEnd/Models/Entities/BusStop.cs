namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class BusStop
    {
        public int Id { get; set; }

        public string StopName { get; set; } = string.Empty;

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public int StopOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}