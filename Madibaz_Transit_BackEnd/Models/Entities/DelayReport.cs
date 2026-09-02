using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Identity.Client;

namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class DelayReport
    {
        public Guid DelayId { get; set; }
        public Guid TripId { get; set; }
        public Trip Trip { get; set; }
        public Guid DriverId { get; set; }
        public Driver Driver { get; set; }
        public required string Reason { get; set; }
        public required string Duration { get; set; }
        public required string Description { get; set; }
        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
    }
}
