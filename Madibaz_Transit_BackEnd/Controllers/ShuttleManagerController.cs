// Controllers/ShuttleManagerController.cs

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Madibaz_Transit_BackEnd.Data;
using Madibaz_Transit_BackEnd.Dtos;
using Madibaz_Transit_BackEnd.Models.Entities;
using Madibaz_Transit_BackEnd.Services;

namespace Madibaz_Transit_BackEnd.Controllers
{
    [ApiController]
    [Route("api/shuttle-manager/drivers")]
    [Authorize(Roles = "ShuttleManager")]
    public class ShuttleManagerController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly PasswordHasher<AppUser> _passwordHasher = new();

        public ShuttleManagerController(AppDbContext db)
        {
            _db = db;
        }

        // Reads the logged-in ShuttleManager's own ID out of their JWT.
        // This is what makes "only see/manage drivers YOU created" work —
        // it's never something the client sends, it comes from the
        // already-verified token.
        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // POST api/shuttle-manager/drivers
        // Driver gets a personal email (Gmail etc.) — no institutional
        // account needed, since trust comes from the ShuttleManager
        // creating the account, not the email domain.
        //
        // Instead of a directly-usable temp password, this generates an
        // ACTIVATION LINK TOKEN. The driver never logs in with a
        // password chosen by someone else — they use the link to set
        // their own password immediately, via the existing
        // /api/auth/reset-password endpoint. Reuses the same fields
        // already built for forgot-password, just triggered at account
        // creation instead of by the user requesting it later.
        [HttpPost]
        public async Task<ActionResult<CreateDriverResponseDto>> CreateDriver(CreateDriverRequestDto dto)
        {
            bool emailExists = await _db.Set<AppUser>().AnyAsync(u => u.Email == dto.Email);
            if (emailExists)
                return Conflict("An account with this email already exists.");

            var driver = new AppUser
            {
                Email = dto.Email,
                FullName = dto.FullName,
                Role = UserRole.Driver,
                IsActive = true,
                MustChangePassword = true, // still true until they actually set one
                CreatedByUserId = CurrentUserId,
                StudentNumber = null
            };

            // No usable password exists yet — only an activation token.
            // A random, never-told-to-anyone password fills the required
            // PasswordHash field so the account is valid, but login can't
            // succeed with it (nobody knows it, including us).
            var unusablePlaceholder = TempPasswordGenerator.Generate(20);
            driver.PasswordHash = _passwordHasher.HashPassword(driver, unusablePlaceholder);

            var activationToken = TempPasswordGenerator.Generate(10);
            driver.PasswordResetTokenHash = _passwordHasher.HashPassword(driver, activationToken);
            driver.PasswordResetTokenExpiresAt = System.DateTime.UtcNow.AddHours(24); // longer window than a forgot-password reset, since it's a one-time onboarding link

            _db.Set<AppUser>().Add(driver);
            await _db.SaveChangesAsync();

            // TODO: write an AuditLog entry here once that table exists —
            // "Driver account created", performed by CurrentUserId, target
            // = driver.AppUserId. Same pattern as the AuditLog design
            // from earlier in this conversation.

            return Ok(new CreateDriverResponseDto
            {
                AppUserId = driver.AppUserId,
                Email = driver.Email,
                FullName = driver.FullName,
                // In production this becomes a real link, e.g.
                // https://madibatransit.co.za/set-password?email=...&token=...
                // emailed directly to the driver. No email system exists
                // yet, so the raw token is returned here for the
                // ShuttleManager to relay manually — same honest
                // stand-in pattern used throughout this project.
                TemporaryPassword = activationToken
            });
        }

        // GET api/shuttle-manager/drivers
        // Only shows drivers THIS shuttle manager created — not every
        // driver in the system.
        [HttpGet]
        public async Task<ActionResult<System.Collections.Generic.IEnumerable<DriverAccountSummaryDto>>> GetMyDrivers()
        {
            var drivers = await _db.Set<AppUser>()
                .Where(u => u.Role == UserRole.Driver && u.CreatedByUserId == CurrentUserId)
                .Select(u => new DriverAccountSummaryDto
                {
                    AppUserId = u.AppUserId,
                    Email = u.Email,
                    FullName = u.FullName,
                    IsActive = u.IsActive,
                    MustChangePassword = u.MustChangePassword
                })
                .ToListAsync();

            return Ok(drivers);
        }

        // PATCH api/shuttle-manager/drivers/5/reset-password
        // For when a driver forgets their password or a handoff went
        // wrong. Generates a NEW temp password, same as account
        // creation — the old password stops working immediately.
        [HttpPatch("{id:int}/reset-password")]
        public async Task<ActionResult<CreateDriverResponseDto>> ResetDriverPassword(int id)
        {
            var driver = await _db.Set<AppUser>().FindAsync(id);
            if (driver == null) return NotFound();

            // Same ownership check as deactivate — a manager can only
            // reset passwords for drivers THEY created.
            if (driver.CreatedByUserId != CurrentUserId)
                return Forbid();

            var newTempPassword = TempPasswordGenerator.Generate();
            driver.PasswordHash = _passwordHasher.HashPassword(driver, newTempPassword);
            driver.MustChangePassword = true; // forces them through the change flow again
            driver.UpdatedAt = System.DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new CreateDriverResponseDto
            {
                AppUserId = driver.AppUserId,
                Email = driver.Email,
                FullName = driver.FullName,
                TemporaryPassword = newTempPassword // shown once, same as creation
            });
        }

        // PATCH api/shuttle-manager/drivers/5/deactivate
        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> DeactivateDriver(int id)
        {
            var driver = await _db.Set<AppUser>().FindAsync(id);
            if (driver == null) return NotFound();

            // A manager can only deactivate drivers THEY created —
            // stops one vendor's manager from disabling another vendor's staff.
            if (driver.CreatedByUserId != CurrentUserId)
                return Forbid();

            driver.IsActive = false;
            driver.UpdatedAt = System.DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}