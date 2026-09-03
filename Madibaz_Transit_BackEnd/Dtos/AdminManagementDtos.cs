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

