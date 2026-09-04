
namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        Boarded,
        Cancelled,
        Expired
    }

    public enum ReservationExpiryReason
    {
        NotConfirmed,
        NoShow
    }

    public class SeatReservations
    {
        public Guid Id { get; set; }

        public int AppUserId { get; set; }

        public AppUser AppUser { get; set; } = null!;

        public Guid TripId { get; set; }

        public Trip Trip { get; set; } = null!;

        public ReservationStatus Status { get; set; }
            = ReservationStatus.Pending;

        public ReservationExpiryReason? ExpiryReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ConfirmedAt { get; set; }

        public DateTime? BoardedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public DateTime? ExpiredAt { get; set; }

        public Guid BoardingToken { get; set; } = Guid.NewGuid();

        public bool BoardingTokenUsed { get; set; }

        public DateTime? BoardingTokenUsedAt { get; set; }

        public int? VerifiedByAppUserId { get; set; }
    }
}