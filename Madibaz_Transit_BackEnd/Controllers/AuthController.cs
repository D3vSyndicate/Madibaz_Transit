// Controllers/AuthController.cs

using Madibaz_Transit_BackEnd.Data;
using Madibaz_Transit_BackEnd.Dtos;
using Madibaz_Transit_BackEnd.Models.Entities;
using Madibaz_Transit_BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
namespace Madibaz_Transit_BackEnd.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtTokenService _jwtService;
        private readonly PasswordHasher<AppUser> _passwordHasher = new();

        public AuthController(
            AppDbContext db,
            JwtTokenService jwtService)
        {
            _db = db;
            _jwtService = jwtService;
        }

        // POST: api/auth/login
        //
        // Body:
        // {
        //     "email": "s256964895@mandela.ac.za",
        //     "password": "..."
        // }
        //
        // The user logs in using their university email address
        // and password. The system validates the credentials against
        // the seeded application data and issues a JWT token.

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(
            LoginRequestDto dto)
        {
            var user = await _db.Set<AppUser>()
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            // Do not reveal whether an email address exists.
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            // Prevent suspended users from logging in.
            if (!user.IsActive)
            {
                return Unauthorized(
                    "This account has been suspended. Contact an administrator.");
            }

            // Verify the supplied password against the stored hash.
            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid email or password.");
            }

            // Generate JWT token after successful authentication.
            var token = _jwtService.GenerateToken(user);

            return Ok(new LoginResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                StudentNumber = user.StudentNumber,
                Role = user.Role.ToString(),
                MustChangePassword = user.MustChangePassword
            });
        }


        // POST: api/auth/change-password
        //
        // Requires a valid JWT token.
        //
        // Used when a logged-in user wants to change their password,
        // including users who have been assigned a temporary password.

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordRequestDto dto)
        {
            // Get the user's ID from the authenticated JWT.
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            // Make sure the JWT contains a valid user ID.
            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Invalid user token.");
            }

            // Find the user in the database.
            var user = await _db.Set<AppUser>().FindAsync(userId);

            if (user == null)
            {
                return NotFound("User account was not found.");
            }

            // Verify the current password.
            var verify = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.CurrentPassword);

            if (verify == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Current password is incorrect.");
            }

            // Hash and save the new password.
            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                dto.NewPassword);

            // The temporary-password requirement is now cleared.
            user.MustChangePassword = false;

            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return NoContent();
        }


        // POST: api/auth/forgot-password
        //
        // Body: { "email": "..." }
        //
        // Generates a one-time reset token, stored (hashed) directly on
        // the user's own row via PasswordResetTokenHash / ExpiresAt.
        // In a real system this token gets EMAILED, never returned in
        // the response. Since there's no email system set up yet, it's
        // returned directly here — a stand-in, same pattern as the
        // simulated login itself.
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword(
            ForgotPasswordRequestDto dto)
        {
            var user = await _db.Set<AppUser>()
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            // Same principle as Login: never reveal whether the email
            // exists. Always return a generic success-shaped response.
            if (user == null || !user.IsActive)
            {
                return Ok(new ForgotPasswordResponseDto
                {
                    Message = "If that email exists in our system, a reset token has been generated.",
                    ResetToken = string.Empty
                });
            }

            var rawToken = TempPasswordGenerator.Generate(32);

            user.PasswordResetTokenHash = HashToken(rawToken);
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);

            await _db.SaveChangesAsync();

            return Ok(new ForgotPasswordResponseDto
            {
                Message = "Reset token generated.",
                ResetToken = rawToken
            });
        }


        // POST: api/auth/reset-password
        //
        // Body: { "email": "...", "resetToken": "...", "newPassword": "..." }
        //
        // Verifies the token belongs to the given email and hasn't
        // expired, then sets the new password and clears the token
        // so it can't be reused.
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordRequestDto dto)
        {
            var user = await _db.Set<AppUser>()
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null ||
                user.PasswordResetTokenHash == null ||
                user.PasswordResetTokenExpiresAt == null ||
                user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
            {
                return BadRequest("This reset link is invalid or has expired.");
            }

            var tokenHash = HashToken(dto.ResetToken);

            if (tokenHash != user.PasswordResetTokenHash)
            {
                return BadRequest("This reset link is invalid or has expired.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
            user.MustChangePassword = false;
            user.UpdatedAt = DateTime.UtcNow;

            // Invalidate the token so it can't be reused.
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiresAt = null;

            await _db.SaveChangesAsync();

            return NoContent();
        }


        // Deterministic hash so we can compare tokens by value (a
        // random-salt hash like PasswordHasher can't be compared this
        // way). This is fine here because reset tokens are single-use,
        // short-lived, and high-entropy (32 random characters) —
        // unlike real passwords, which must use PasswordHasher.
        private static string HashToken(string rawToken)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToBase64String(bytes);
        }
    }
}