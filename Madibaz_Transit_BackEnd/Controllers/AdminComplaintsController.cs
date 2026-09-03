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

            // Simple state check â€” can't "resolve" something already closed out.
            if (c.Status is ComplaintStatus.Resolved or ComplaintStatus.Rejected)
                return BadRequest($"This complaint is already '{c.Status}' and cannot be updated further.");

            c.Status = newStatus;
            c.ResolutionNotes = dto.ResolutionNotes;

            if (newStatus is ComplaintStatus.Resolved or ComplaintStatus.Rejected)
                c.ResolvedAt = System.DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // TODO: AuditLog entry â€” "ComplaintResolved", once that table exists.
            return NoContent();
        }
    }
}

