// Models/Entities/ComplainTickets.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public enum ComplaintStatus
    {
        Open,
        InReview,
        Resolved,
        Rejected
    }

    public class ComplainTickets
    {
       [Key]
        public int ComplaintTicketId { get; set; }
        public int? OriginalIncidentReportId { get; set; }
        public string? SubmittedByStudentName { get; set; }

        [Required, MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public ComplaintStatus Status { get; set; } = ComplaintStatus.Open;

        [MaxLength(1000)]
        public string? ResolutionNotes { get; set; }

        public int? ResolvedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
    }
}