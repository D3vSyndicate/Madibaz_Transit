namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class Driver
    {
        public Guid DriverId { get; set; }
        public Guid AppUserId { get; set; }
        public required string EmployeeNumber { get; set; }
        public required string LicenseNumber { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

    }
}
