namespace Madibaz_Transit_BackEnd.Dtos
{
    public class CreateBookingDto
    {
        public Guid TripId { get; set; }
    }

    public class ConfirmAttendanceDto
    {
        public bool IsComing { get; set; }
    }

    public class BookingDto
    {
        public Guid Id { get; set; }

        public Guid TripId { get; set; }

        public string RouteName { get; set; } = string.Empty;

        public string RouteCode { get; set; } = string.Empty;

        public DateTime DepartureTime { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? BoardingToken { get; set; }

        public DateTime CreatedAt { get; set; }

        public int QueuePosition { get; set; }

        public bool AttendanceConfirmed { get; set; }

        public bool ConfirmationRequired { get; set; }
    }
}