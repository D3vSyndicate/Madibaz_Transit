using System.ComponentModel.DataAnnotations;

namespace Madibaz_Transit_BackEnd.Models.Entities
{


    public class DriverShift
    {
        public Guid DriverShiftId { get; set; }
        public Guid DriverId { get; set; }
        public Guid BusId { get; set; }
        public Guid RouteId { get; set; }
        public Guid ScheduleId { get; set; }
        public DateTime ShiftStart { get; set; }
        public DateTime? ShiftEnd { get; set; }
        [Required]
        public ShiftStatus ShiftStatus { get; set; }
    }

  public enum ShiftStatus
{
    Active,
    Ended
}

}

