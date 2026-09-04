namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class Marshal
    {
        public Guid MarshalId { get; set; }

        public int UserId { get; set; }
        public AppUser? User { get; set; }

        public string Phone { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime DateAssigned { get; set; }
    }
}