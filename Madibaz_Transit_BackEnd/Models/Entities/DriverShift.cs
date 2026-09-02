using System.ComponentModel.DataAnnotations;

namespace Madibaz_Transit_BackEnd.Models.Entities
{

    public class DriverShift
    {
        public Guid DriverShiftId { get; set; }

        public int DriverId { get; set; }

        public int BusId { get; set; }

        public int RouteId { get; set; }

        public DateTime ShiftStart { get; set; }

        public DateTime? ShiftEnd { get; set; }

        [Required]
        public Status ShiftStatus { get; set; } 
    }

    public enum Status
    {
        Active,Ended
    }
}

