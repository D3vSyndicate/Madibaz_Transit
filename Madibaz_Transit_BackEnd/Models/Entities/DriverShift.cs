using System;

namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class DriverShift
    {
        public Guid DriverShiftId { get; set; }

        public Guid DriverId { get; set; }
        public Driver Driver { get; set; } = null!;

        public int BusId { get; set; }
        public Bus Bus { get; set; } = null!;

        public Guid RouteId { get; set; }
        public TransitRoute Route { get; set; } = null!;

        public int ScheduleId { get; set; }
        public Schedule Schedule { get; set; } = null!;

        public DateTime ShiftDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}