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

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? StudentNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool MustChangePassword { get; set; }
    }

    public class ChangePasswordRequestDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
    }

    // Step 1 of self-service reset: "I forgot my password"
    public class ForgotPasswordRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    // In a real system this token gets EMAILED, never returned in the
    // response. Since there's no email system here yet, it's returned
    // directly — a stand-in, same pattern as the simulated login itself.
    public class ForgotPasswordResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string ResetToken { get; set; } = string.Empty;
    }

    // Step 2: "here's my token, set my new password"
    public class ResetPasswordRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string ResetToken { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
    }
}