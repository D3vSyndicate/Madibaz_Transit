using System;
using System.Collections.Generic;

namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class Driver
    {
        public Guid DriverId { get; set; }

        // AppUser relationship
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; } = null!;

        public string LicenseNumber { get; set; } = string.Empty;

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Driver -> DriverShifts
        public ICollection<DriverShift> DriverShifts { get; set; }
            = new List<DriverShift>();
    }
}