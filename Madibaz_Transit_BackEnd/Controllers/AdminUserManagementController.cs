// Controllers/AdminUserManagementController.cs
// Owner: Admin
//
// Separate from AdminUsersController.cs (which only creates ShuttleManager
// accounts) â€” this one is for viewing and suspending EXISTING accounts
// across all roles. Two different jobs, kept in two files on purpose.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Madibaz_Transit_BackEnd.Data;
using Madibaz_Transit_BackEnd.Dtos;
using Madibaz_Transit_BackEnd.Models.Entities;

namespace Madibaz_Transit_BackEnd.Controllers
{
    [ApiController]
    [Route("api/admin/user-management")]
    [Authorize(Roles = "Admin")]
    public class AdminUserManagementController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AdminUserManagementController(AppDbContext db)
        {
            _db = db;
        }

        // GET api/admin/user-management
        // GET api/admin/user-management?role=Driver
        [HttpGet]
        public async Task<ActionResult<System.Collections.Generic.IEnumerable<UserSummaryDto>>> GetAllUsers(
            [FromQuery] string? role)
        {
            var query = _db.Set<AppUser>().AsQueryable();

            if (!string.IsNullOrEmpty(role))
            {
                if (!System.Enum.TryParse<UserRole>(role, true, out var roleFilter))
                    return BadRequest($"Invalid role '{role}'.");

                query = query.Where(u => u.Role == roleFilter);
            }

            var users = await query
                .OrderBy(u => u.Role).ThenBy(u => u.FullName)
                .Select(u => new UserSummaryDto
                {
                    AppUserId = u.AppUserId,
                    Email = u.Email,
                    FullName = u.FullName,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // PATCH api/admin/user-management/5/suspend
        [HttpPatch("{id:int}/suspend")]
        public async Task<IActionResult> SuspendUser(int id)
        {
            var user = await _db.Set<AppUser>().FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = false;
            user.UpdatedAt = System.DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // TODO: AuditLog entry â€” "UserSuspended", once that table exists.
            return NoContent();
        }

        // PATCH api/admin/user-management/5/reactivate
        [HttpPatch("{id:int}/reactivate")]
        public async Task<IActionResult> ReactivateUser(int id)
        {
            var user = await _db.Set<AppUser>().FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = true;
            user.UpdatedAt = System.DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}

