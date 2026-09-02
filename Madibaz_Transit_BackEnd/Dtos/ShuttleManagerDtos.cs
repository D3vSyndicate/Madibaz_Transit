// Dtos/ShuttleManagerDtos.cs

using System.ComponentModel.DataAnnotations;

namespace Madibaz_Transit_BackEnd.Dtos
{
    // ShuttleManager provides ONLY these two things — never a password.
    // The system generates that.
    public class CreateDriverRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;
    }

    // Shown ONCE, right after creation. This is the only time the
    // plain temporary password exists anywhere outside the driver's head.
    public class CreateDriverResponseDto
    {
        public int AppUserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string TemporaryPassword { get; set; } = string.Empty;
    }

    public class DriverAccountSummaryDto
    {
        public int AppUserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool MustChangePassword { get; set; }
    }
}