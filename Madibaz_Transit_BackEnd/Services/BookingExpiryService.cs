using Madibaz_Transit_BackEnd.Data;
using Madibaz_Transit_BackEnd.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Madibaz_Transit_BackEnd.Services
{
    public class BookingExpiryService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public BookingExpiryService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiredBookings();
                }
                catch
                {
                    // Keep the background service running if
                    // one processing cycle encounters an error.
                }

                await Task.Delay(
                    TimeSpan.FromMinutes(1),
                    stoppingToken);
            }
        }

        private async Task ProcessExpiredBookings()
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var now = DateTime.UtcNow;

            // Find confirmed reservations where the student
            // has not confirmed attendance and departure is
            // within the next 15 minutes.
            var reservations = await db.SeatReservations
                .Include(r => r.Trip)
                .Where(r =>
                    r.Status == ReservationStatus.Confirmed &&
                    r.ConfirmedAt == null &&
                    r.Trip.ScheduledStart <= now.AddMinutes(15) &&
                    r.Trip.ScheduledStart > now)
                .ToListAsync();

            foreach (var reservation in reservations)
            {
                var tripId = reservation.TripId;

                reservation.Status = ReservationStatus.Expired;
                reservation.BoardingToken = Guid.Empty;
                reservation.ExpiredAt = now;
                reservation.ExpiryReason =
                    ReservationExpiryReason.NotConfirmed;

                await PromoteNextStudent(db, tripId);
            }

            if (reservations.Count > 0)
            {
                await db.SaveChangesAsync();
            }
        }

        private async Task PromoteNextStudent(
            AppDbContext db,
            Guid tripId)
        {
            var trip = await db.Trips
                .Include(t => t.Bus)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip == null)
                return;

            var reservedSeats = await db.SeatReservations
                .CountAsync(r =>
                    r.TripId == tripId &&
                    (r.Status == ReservationStatus.Pending ||
                     r.Status == ReservationStatus.Confirmed ||
                     r.Status == ReservationStatus.Boarded));

            if (reservedSeats >= trip.Bus.Capacity)
                return;

            var nextStudent = await db.ActiveQueues
                .Where(q =>
                    q.TripId == tripId &&
                    q.Status == QueueEntryStatus.Waiting)
                .OrderBy(q => q.Position)
                .ThenBy(q => q.JoinedAt)
                .FirstOrDefaultAsync();

            if (nextStudent == null)
                return;

            var newReservation = new SeatReservations
            {
                Id = Guid.NewGuid(),
                AppUserId = nextStudent.AppUserId,
                TripId = tripId,
                Status = ReservationStatus.Confirmed,
                ConfirmedAt = null,
                BoardingToken = Guid.NewGuid(),
                BoardingTokenUsed = false
            };

            nextStudent.Status = QueueEntryStatus.Promoted;

            db.SeatReservations.Add(newReservation);

            await RecalculateQueuePositions(db, tripId);
        }

        private async Task RecalculateQueuePositions(
            AppDbContext db,
            Guid tripId)
        {
            var queue = await db.ActiveQueues
                .Where(q =>
                    q.TripId == tripId &&
                    q.Status == QueueEntryStatus.Waiting)
                .OrderBy(q => q.JoinedAt)
                .ToListAsync();

            for (int i = 0; i < queue.Count; i++)
            {
                queue[i].Position = i + 1;
            }
        }
    }
}
