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

                // Check every minute.
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

            var bookings = await db.Bookings
                .Include(b => b.ScheduledTrip)
                .Where(b =>
                    b.Status == "Confirmed" &&
                    !b.AttendanceConfirmed &&
                    b.ScheduledTrip.DepartureTime <=
                        now.AddMinutes(15) &&
                    b.ScheduledTrip.DepartureTime > now)
                .ToListAsync();

            foreach (var booking in bookings)
            {
                booking.Status = "Expired";
                booking.BoardingToken = null;

                await PromoteNextStudent(
                    db,
                    booking.ScheduledTripId);
            }

            if (bookings.Count > 0)
            {
                await db.SaveChangesAsync();
            }
        }

        private async Task PromoteNextStudent(
            AppDbContext db,
            int scheduledTripId)
        {
            var trip = await db.ScheduledTrips
                .Include(t => t.Bus)
                .FirstOrDefaultAsync(
                    t => t.Id == scheduledTripId);

            if (trip == null)
                return;

            var confirmedCount = await db.Bookings
                .CountAsync(b =>
                    b.ScheduledTripId == scheduledTripId &&
                    b.Status == "Confirmed");

            if (confirmedCount >= trip.Bus.Capacity)
                return;

            var nextStudent = await db.Bookings
                .Where(b =>
                    b.ScheduledTripId == scheduledTripId &&
                    b.Status == "Queued")
                .OrderBy(b => b.CreatedAt)
                .FirstOrDefaultAsync();

            if (nextStudent == null)
                return;

            nextStudent.Status = "Confirmed";
            nextStudent.BoardingToken =
                Guid.NewGuid().ToString("N");
            nextStudent.AttendanceConfirmed = false;
        }
    }
}