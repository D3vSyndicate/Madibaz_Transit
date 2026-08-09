using System.ComponentModel.DataAnnotations;

namespace Madibaz_Transit_BackEnd.Models.Entities
{

    public class DriverShift
    {
        public int Id { get; set; }

        public int DriverId { get; set; }

        public int BusId { get; set; }

        public int RouteId { get; set; }

        public DateTime ShiftStart { get; set; }

        public DateTime? ShiftEnd { get; set; }

        [Required]
        public string ShiftStatus { get; set; } = "Active";
    }
}

