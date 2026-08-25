// Dtos/AuthDtos.cs

using System.ComponentModel.DataAnnotations;

namespace Madibaz_Transit_BackEnd.Dtos
{
    public class LoginRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    // What we send back. Role comes from OUR database — the frontend
    // uses this purely to decide which dashboard to route to, it never
    // decides the role itself.
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? StudentNumber { get; set; }
        public string Role { get; set; } = string.Empty;

        // If true, the frontend MUST show the change-password screen
        // before letting the user do anything else — this is how a
        // ShuttleManager-created driver's temp password gets replaced.
        public bool MustChangePassword { get; set; }
    }

    public class ChangePasswordRequestDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
    }
}