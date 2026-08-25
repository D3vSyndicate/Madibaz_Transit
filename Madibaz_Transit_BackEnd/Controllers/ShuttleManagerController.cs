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
        [HttpPost]
        public async Task<ActionResult<CreateDriverResponseDto>> CreateDriver(CreateDriverRequestDto dto)
        {
            bool emailExists = await _db.Set<AppUser>().AnyAsync(u => u.Email == dto.Email);
            if (emailExists)
                return Conflict("An account with this email already exists.");

            var tempPassword = TempPasswordGenerator.Generate();

            var driver = new AppUser
            {
                Email = dto.Email,
                FullName = dto.FullName,
                Role = UserRole.Driver,
                IsActive = true,
                MustChangePassword = true, // forces password change on first login
                CreatedByUserId = CurrentUserId,
                StudentNumber = null
            };
            driver.PasswordHash = _passwordHasher.HashPassword(driver, tempPassword);

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
                TemporaryPassword = tempPassword // shown ONCE, right here, never again
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