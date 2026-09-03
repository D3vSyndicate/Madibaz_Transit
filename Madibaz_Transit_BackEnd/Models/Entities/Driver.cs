namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class Driver
    {
        public Guid DriverId { get; set; }
        public Guid UserId { get; set; }
        public AppUser? User { get; set; }
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string EmployeeNumber { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Status { get; set; } = "Available";

        public bool IsActive { get; set; } = true;
    }
}