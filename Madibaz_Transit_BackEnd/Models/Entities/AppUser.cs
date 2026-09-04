using System.ComponentModel.DataAnnotations.Schema;

namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public enum UserRole
    {
        Student,
        Driver,
        Marshal,
        Admin,
        ShuttleManager
    }

    public class AppUser
    {
        public int AppUserId { get; set; }

        public string? StudentNumber { get; set; }

        public string? PasswordResetTokenHash { get; set; }

        public DateTime? PasswordResetTokenExpiresAt { get; set; }

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Student;

        public bool IsActive { get; set; } = true;

        public bool MustChangePassword { get; set; } = false;

        public int? CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string PlainPassword { get; set; } = string.Empty;

        // Navigation properties
        public AppUser? CreatedByUser { get; set; }

        public ICollection<StudentProfiles> StudentProfiles { get; set; }
            = new List<StudentProfiles>();

        public ICollection<Driver> Drivers { get; set; }
            = new List<Driver>();

        public ICollection<Marshal> Marshals { get; set; }
            = new List<Marshal>();

        public ICollection<ActiveQueues> ActiveQueues { get; set; }
            = new List<ActiveQueues>();

        public ICollection<SeatReservations> SeatReservations { get; set; }
            = new List<SeatReservations>();

        public ICollection<ComplainTickets> ComplaintTickets { get; set; }
            = new List<ComplainTickets>();
    }
}