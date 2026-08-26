namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class ScheduledTrip
    {
        public int Id { get; set; }

        public int TransitRouteId { get; set; }

        public int BusId { get; set; }

        public DateTime DepartureTime { get; set; }

        public string Status { get; set; } = "Scheduled";

        public bool IsActive { get; set; } = true;

        public TransitRoute TransitRoute { get; set; } = null!;

        public Bus Bus { get; set; } = null!;
    }
}