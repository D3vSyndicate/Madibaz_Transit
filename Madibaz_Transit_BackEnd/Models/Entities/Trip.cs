
namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class Trip
    {
        public Guid TripId { get; set; }

        public Guid DriverShiftId { get; set; }

        public Guid TransitRouteId { get; set; }

        public int BusId { get; set; }

        public DriverShift DriverShift { get; set; } = null!;

        public TransitRoute TransitRoute { get; set; } = null!;

        public Bus Bus { get; set; } = null!;

        public required string Status { get; set; }

        public DateTime ScheduledStart { get; set; }

        public DateTime? ActualStart { get; set; }

        public DateTime? ActualEnd { get; set; }

        public DateTime? ArrivedAt { get; set; }

        public ICollection<SeatReservations> SeatReservations { get; set; }
            = new List<SeatReservations>();

        public ICollection<ActiveQueues> ActiveQueues { get; set; }
            = new List<ActiveQueues>();

        public ICollection<TripStatusHistory> StatusHistory { get; set; }
            = new List<TripStatusHistory>();
    }
}