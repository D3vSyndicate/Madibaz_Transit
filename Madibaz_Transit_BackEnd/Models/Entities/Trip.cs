namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class Trip
    {
        public Guid TripId { get; set; }
        public Guid DriverShiftId { get; set; }
        public Guid TransitRouteId { get; set; }
        public Guid BusId { get; set; }
        public DriverShift DriverShift { get; set; }
        public TransitRoute TransitRoute { get; set; }
        public Bus Bus { get; set; }
        public required string Status { get; set; }
        public required string ScheduledStart { get; set; }
        public required string ActualStart { get; set; }
        public required string ActualEnd { get; set; }

    }
}
