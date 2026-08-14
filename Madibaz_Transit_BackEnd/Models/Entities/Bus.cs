namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class Bus
    {
        public int Id { get; set; }

        public string RegistrationNumber { get; set; } = string.Empty;

        public string FleetNumber { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public string Status { get; set; } = "Available";

        public bool IsActive { get; set; } = true;
    }
}