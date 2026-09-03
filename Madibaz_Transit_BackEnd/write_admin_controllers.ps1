Write-Host '=== Writing 3 files ===' -ForegroundColor Cyan

$content_AdminManagementDtos_cs = @'
// Dtos/AdminManagementDtos.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace Madibaz_Transit_BackEnd.Dtos
{
    // ---------- USER MANAGEMENT ----------

    public class UserSummaryDto
    {
        public int AppUserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ---------- COMPLAINTS ----------

    public class ComplaintSummaryDto
    {
        public int ComplaintTicketId { get; set; }
        public string? SubmittedByStudentName { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ComplaintDetailDto
    {
        public int ComplaintTicketId { get; set; }
        public string? SubmittedByStudentName { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ResolutionNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class ResolveComplaintRequestDto
    {
        [Required]
        public string Status { get; set; } = string.Empty; // "Resolved" | "Rejected" | "InReview"

        [MaxLength(1000)]
        public string? ResolutionNotes { get; set; }
    }
}

'@
Set-Content -Path "Dtos/AdminManagementDtos.cs" -Value $content_AdminManagementDtos_cs -Encoding UTF8
$lineCount = (Get-Content "Dtos/AdminManagementDtos.cs").Count
Write-Host "Wrote Dtos/AdminManagementDtos.cs - now $lineCount lines" -ForegroundColor Green

$content_AdminUserManagementController_cs = @'
// Controllers/AdminUserManagementController.cs
// Owner: Admin
//
// Separate from AdminUsersController.cs (which only creates ShuttleManager
// accounts) — this one is for viewing and suspending EXISTING accounts
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

            // TODO: AuditLog entry — "UserSuspended", once that table exists.
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

'@
Set-Content -Path "Controllers/AdminUserManagementController.cs" -Value $content_AdminUserManagementController_cs -Encoding UTF8
$lineCount = (Get-Content "Controllers/AdminUserManagementController.cs").Count
Write-Host "Wrote Controllers/AdminUserManagementController.cs - now $lineCount lines" -ForegroundColor Green

$content_AdminComplaintsController_cs = @'
// Controllers/AdminComplaintsController.cs
// Owner: Admin
// Note: uses the ComplainTickets entity/DbSet name exactly as it exists
// in the real project (kept as-is to avoid an unnecessary rename).

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
    [Route("api/admin/complaints")]
    [Authorize(Roles = "Admin")]
    public class AdminComplaintsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AdminComplaintsController(AppDbContext db)
        {
            _db = db;
        }

        // GET api/admin/complaints
        // GET api/admin/complaints?status=Open
        [HttpGet]
        public async Task<ActionResult<System.Collections.Generic.IEnumerable<ComplaintSummaryDto>>> GetAll(
            [FromQuery] string? status)
        {
            var query = _db.ComplainTickets.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (!System.Enum.TryParse<ComplaintStatus>(status, true, out var statusFilter))
                    return BadRequest($"Invalid status '{status}'.");

                query = query.Where(c => c.Status == statusFilter);
            }

            var complaints = await query
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ComplaintSummaryDto
                {
                    ComplaintTicketId = c.ComplaintTicketId,
                    SubmittedByStudentName = c.SubmittedByStudentName,
                    Subject = c.Subject,
                    Status = c.Status.ToString(),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(complaints);
        }

        // GET api/admin/complaints/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ComplaintDetailDto>> GetById(int id)
        {
            var c = await _db.ComplainTickets.FindAsync(id);
            if (c == null) return NotFound();

            return Ok(new ComplaintDetailDto
            {
                ComplaintTicketId = c.ComplaintTicketId,
                SubmittedByStudentName = c.SubmittedByStudentName,
                Subject = c.Subject,
                Description = c.Description,
                Status = c.Status.ToString(),
                ResolutionNotes = c.ResolutionNotes,
                CreatedAt = c.CreatedAt,
                ResolvedAt = c.ResolvedAt
            });
        }

        // PATCH api/admin/complaints/5/resolve
        // Body: { "status": "Resolved", "resolutionNotes": "..." }
        [HttpPatch("{id:int}/resolve")]
        public async Task<IActionResult> Resolve(int id, ResolveComplaintRequestDto dto)
        {
            var c = await _db.ComplainTickets.FindAsync(id);
            if (c == null) return NotFound();

            if (!System.Enum.TryParse<ComplaintStatus>(dto.Status, true, out var newStatus))
                return BadRequest($"Invalid status '{dto.Status}'. Must be Open, InReview, Resolved, or Rejected.");

            // Simple state check — can't "resolve" something already closed out.
            if (c.Status is ComplaintStatus.Resolved or ComplaintStatus.Rejected)
                return BadRequest($"This complaint is already '{c.Status}' and cannot be updated further.");

            c.Status = newStatus;
            c.ResolutionNotes = dto.ResolutionNotes;

            if (newStatus is ComplaintStatus.Resolved or ComplaintStatus.Rejected)
                c.ResolvedAt = System.DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // TODO: AuditLog entry — "ComplaintResolved", once that table exists.
            return NoContent();
        }
    }
}

'@
Set-Content -Path "Controllers/AdminComplaintsController.cs" -Value $content_AdminComplaintsController_cs -Encoding UTF8
$lineCount = (Get-Content "Controllers/AdminComplaintsController.cs").Count
Write-Host "Wrote Controllers/AdminComplaintsController.cs - now $lineCount lines" -ForegroundColor Green

Write-Host '=== Now run: dotnet build ===' -ForegroundColor Cyan