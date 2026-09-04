namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class StudentProfiles
    {
        public int Id { get; set; }

        public int AppUserId { get; set; }

        public AppUser AppUser { get; set; } = null!;

        public string Faculty { get; set; } = string.Empty;

        public string? Programme { get; set; }
    }
}