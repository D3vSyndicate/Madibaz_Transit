namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class Schedule
    {
        public int ScheduleId { get; set; }

        public int RouteId { get; set; }

        public int ShuttleId { get; set; }

        public TimeSpan DepartureTime { get; set; }

        public string DayOfWeek { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}