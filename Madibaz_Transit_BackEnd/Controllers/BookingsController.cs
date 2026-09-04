using System.Security.Claims;
using Madibaz_Transit_BackEnd.Data;
using Madibaz_Transit_BackEnd.Dtos;
using Madibaz_Transit_BackEnd.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Madibaz_Transit_BackEnd.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    [Authorize(Roles = "Student")]
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BookingsController(AppDbContext db)
        {
            _db = db;
        }

        // Create a booking or join the queue
        [HttpPost]
        public async Task<IActionResult> CreateBooking(CreateBookingDto dto)
        {
            var userId = GetUserId();

            var trip = await _db.Trips
                .Include(t => t.Bus)
                .Include(t => t.TransitRoute)
                .FirstOrDefaultAsync(t => t.TripId == dto.TripId);

            if (trip == null)
                return NotFound("Trip not found.");

            if (trip.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                trip.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("This trip is no longer available for booking.");
            }

            // New bookings close 15 minutes before departure.
            if (trip.ScheduledStart <= DateTime.UtcNow.AddMinutes(15))
            {
                return BadRequest(
                    "Bookings are closed because this shuttle departs in less than 15 minutes.");
            }

            // Check for an existing reservation.
            var existingReservation = await _db.SeatReservations
                .AnyAsync(r =>
                    r.AppUserId == userId &&
                    r.TripId == dto.TripId &&
                    (r.Status == ReservationStatus.Pending ||
                     r.Status == ReservationStatus.Confirmed ||
                     r.Status == ReservationStatus.Boarded));

            // Check for an existing queue entry.
            var existingQueueEntry = await _db.ActiveQueues
                .AnyAsync(q =>
                    q.AppUserId == userId &&
                    q.TripId == dto.TripId &&
                    q.Status == QueueEntryStatus.Waiting);

            if (existingReservation || existingQueueEntry)
            {
                return Conflict(
                    "You already have a booking or queue position for this trip.");
            }

            // Count confirmed reservations.
            var confirmedBookings = await _db.SeatReservations
                .CountAsync(r =>
                    r.TripId == dto.TripId &&
                    (r.Status == ReservationStatus.Pending ||
                     r.Status == ReservationStatus.Confirmed));

            // There is still space.
            if (confirmedBookings < trip.Bus.Capacity)
            {
                var reservation = new SeatReservations
                {
                    AppUserId = userId,
                    TripId = dto.TripId,
                    Status = ReservationStatus.Confirmed,
                    ConfirmedAt = DateTime.UtcNow,
                    BoardingToken = Guid.NewGuid(),
                    BoardingTokenUsed = false
                };

                _db.SeatReservations.Add(reservation);

                await _db.SaveChangesAsync();

                return Ok(await BuildBookingDto(
                    reservation.Id,
                    userId));
            }

            // Bus is full, so add student to the queue.
            var queuePosition = await _db.ActiveQueues
                .CountAsync(q =>
                    q.TripId == dto.TripId &&
                    q.Status == QueueEntryStatus.Waiting) + 1;

            var queueEntry = new ActiveQueues
            {
                AppUserId = userId,
                TripId = dto.TripId,
                Position = queuePosition,
                JoinedAt = DateTime.UtcNow,
                Status = QueueEntryStatus.Waiting
            };

            _db.ActiveQueues.Add(queueEntry);

            await _db.SaveChangesAsync();

            return Ok(new
            {
                Id = queueEntry.Id,
                TripId = queueEntry.TripId,
                Status = "Queued",
                QueuePosition = queueEntry.Position,
                CreatedAt = queueEntry.JoinedAt,
                RouteName = trip.TransitRoute.RouteName,
                RouteCode = trip.TransitRoute.RouteCode,
                DepartureTime = trip.ScheduledStart
            });
        }

        // Get all bookings belonging to the logged-in student
        [HttpGet("my")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = GetUserId();

            var reservations = await _db.SeatReservations
                .Include(r => r.Trip)
                    .ThenInclude(t => t.TransitRoute)
                .Where(r => r.AppUserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var queueEntries = await _db.ActiveQueues
                .Include(q => q.Trip)
                    .ThenInclude(t => t.TransitRoute)
                .Where(q =>
                    q.AppUserId == userId &&
                    q.Status == QueueEntryStatus.Waiting)
                .OrderByDescending(q => q.JoinedAt)
                .ToListAsync();

            var result = new List<BookingDto>();

            foreach (var reservation in reservations)
            {
                result.Add(await BuildBookingDto(
                    reservation.Id,
                    userId));
            }

            foreach (var queue in queueEntries)
            {
                result.Add(new BookingDto
                {
                    Id = queue.Id,
                    TripId = queue.TripId,
                    RouteName = queue.Trip.TransitRoute.RouteName,
                    RouteCode = queue.Trip.TransitRoute.RouteCode,
                    DepartureTime = queue.Trip.ScheduledStart,
                    Status = "Queued",
                    BoardingToken = Guid.Empty.ToString(),
                    CreatedAt = queue.JoinedAt,
                    QueuePosition = queue.Position,
                    AttendanceConfirmed = false,
                    ConfirmationRequired = false
                });
            }

            return Ok(result
                .OrderByDescending(x => x.CreatedAt)
                .ToList());
        }

        // Confirm whether the student is still coming.
        // Confirmation is available from 30 minutes
        // before departure until 15 minutes before departure.
        [HttpPost("{id}/confirm-attendance")]
        public async Task<IActionResult> ConfirmAttendance(
            Guid id,
            ConfirmAttendanceDto dto)
        {
            var userId = GetUserId();

            var reservation = await _db.SeatReservations
                .Include(r => r.Trip)
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.AppUserId == userId);

            if (reservation == null)
                return NotFound("Booking not found.");

            if (reservation.Status != ReservationStatus.Confirmed)
            {
                return BadRequest(
                    "Only confirmed bookings can confirm attendance.");
            }

            var departureTime = reservation.Trip.ScheduledStart;
            var now = DateTime.UtcNow;

            var confirmationStart = departureTime.AddMinutes(-30);
            var confirmationEnd = departureTime.AddMinutes(-15);

            if (now < confirmationStart)
            {
                return BadRequest(
                    "Attendance confirmation is not available yet.");
            }

            if (now >= confirmationEnd)
            {
                return BadRequest(
                    "The attendance confirmation period has ended.");
            }

            // Student says they are NOT coming.
            if (!dto.IsComing)
            {
                reservation.Status = ReservationStatus.Cancelled;
                reservation.CancelledAt = now;

                await PromoteNextStudent(reservation.TripId);

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Your booking has been cancelled."
                });
            }

            // Student confirms that they ARE coming.
            reservation.ConfirmedAt = now;

            await _db.SaveChangesAsync();

            return Ok(await BuildBookingDto(
                reservation.Id,
                userId));
        }

        // Cancel a booking or queue position
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var userId = GetUserId();

            var reservation = await _db.SeatReservations
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.AppUserId == userId);

            if (reservation != null)
            {
                if (reservation.Status == ReservationStatus.Cancelled)
                    return BadRequest("Booking is already cancelled.");

                if (reservation.Status == ReservationStatus.Expired)
                    return BadRequest("This booking has already expired.");

                var wasConfirmed =
                    reservation.Status == ReservationStatus.Confirmed;

                reservation.Status = ReservationStatus.Cancelled;
                reservation.CancelledAt = DateTime.UtcNow;

                if (wasConfirmed)
                {
                    await PromoteNextStudent(reservation.TripId);
                }

                await _db.SaveChangesAsync();

                return NoContent();
            }

            var queueEntry = await _db.ActiveQueues
                .FirstOrDefaultAsync(q =>
                    q.Id == id &&
                    q.AppUserId == userId &&
                    q.Status == QueueEntryStatus.Waiting);

            if (queueEntry == null)
                return NotFound("Booking or queue position not found.");

            queueEntry.Status = QueueEntryStatus.Cancelled;

            await _db.SaveChangesAsync();

            return NoContent();
        }

        // Get one booking belonging to the logged-in student
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(Guid id)
        {
            var userId = GetUserId();

            var reservation = await _db.SeatReservations
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.AppUserId == userId);

            if (reservation != null)
            {
                return Ok(await BuildBookingDto(
                    reservation.Id,
                    userId));
            }

            var queueEntry = await _db.ActiveQueues
                .Include(q => q.Trip)
                    .ThenInclude(t => t.TransitRoute)
                .FirstOrDefaultAsync(q =>
                    q.Id == id &&
                    q.AppUserId == userId);

            if (queueEntry == null)
                return NotFound("Booking or queue position not found.");

            return Ok(new BookingDto
            {
                Id = queueEntry.Id,
                TripId = queueEntry.TripId,
                RouteName = queueEntry.Trip.TransitRoute.RouteName,
                RouteCode = queueEntry.Trip.TransitRoute.RouteCode,
                DepartureTime = queueEntry.Trip.ScheduledStart,
                Status = "Queued",
                BoardingToken = Guid.Empty.ToString(),
                CreatedAt = queueEntry.JoinedAt,
                QueuePosition = queueEntry.Position,
                AttendanceConfirmed = false,
                ConfirmationRequired = false
            });
        }

        // Expire confirmed reservations that were not confirmed
        // during the attendance confirmation window.
        [HttpPost("process-expired")]
        public async Task<IActionResult> ProcessExpiredBookings()
        {
            var now = DateTime.UtcNow;

            var reservations = await _db.SeatReservations
                .Include(r => r.Trip)
                .Where(r =>
                    r.Status == ReservationStatus.Confirmed &&
                    r.ConfirmedAt == null &&
                    r.Trip.ScheduledStart <= now.AddMinutes(15) &&
                    r.Trip.ScheduledStart > now)
                .ToListAsync();

            foreach (var reservation in reservations)
            {
                reservation.Status = ReservationStatus.Expired;
                reservation.BoardingToken = Guid.Empty;
                reservation.ExpiryReason =
                    ReservationExpiryReason.NotConfirmed;
                reservation.ExpiredAt = now;

                await PromoteNextStudent(reservation.TripId);
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                ExpiredBookings = reservations.Count
            });
        }

        // Promote the first queued student when a confirmed
        // booking becomes available.
        private async Task PromoteNextStudent(Guid tripId)
        {
            var trip = await _db.Trips
                .Include(t => t.Bus)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip == null)
                return;

            var confirmedCount = await _db.SeatReservations
                .CountAsync(r =>
                    r.TripId == tripId &&
                    (r.Status == ReservationStatus.Pending ||
                     r.Status == ReservationStatus.Confirmed));

            if (confirmedCount >= trip.Bus.Capacity)
                return;

            var nextInQueue = await _db.ActiveQueues
                .Where(q =>
                    q.TripId == tripId &&
                    q.Status == QueueEntryStatus.Waiting)
                .OrderBy(q => q.Position)
                .ThenBy(q => q.JoinedAt)
                .FirstOrDefaultAsync();

            if (nextInQueue == null)
                return;

            var reservation = new SeatReservations
            {
                AppUserId = nextInQueue.AppUserId,
                TripId = tripId,
                Status = ReservationStatus.Confirmed,
                ConfirmedAt = null,
                BoardingToken = Guid.NewGuid(),
                BoardingTokenUsed = false
            };

            _db.SeatReservations.Add(reservation);

            nextInQueue.Status = QueueEntryStatus.Promoted;

            await RecalculateQueuePositions(tripId);
        }

        private async Task RecalculateQueuePositions(Guid tripId)
        {
            var waitingEntries = await _db.ActiveQueues
                .Where(q =>
                    q.TripId == tripId &&
                    q.Status == QueueEntryStatus.Waiting)
                .OrderBy(q => q.JoinedAt)
                .ToListAsync();

            for (var i = 0; i < waitingEntries.Count; i++)
            {
                waitingEntries[i].Position = i + 1;
            }
        }

        private async Task<BookingDto> BuildBookingDto(
            Guid bookingId,
            int userId)
        {
            var booking = await _db.SeatReservations
                .Include(r => r.Trip)
                    .ThenInclude(t => t.TransitRoute)
                .FirstAsync(r =>
                    r.Id == bookingId &&
                    r.AppUserId == userId);

            var queuePosition = 0;

            var queueEntry = await _db.ActiveQueues
                .FirstOrDefaultAsync(q =>
                    q.AppUserId == userId &&
                    q.TripId == booking.TripId &&
                    q.Status == QueueEntryStatus.Waiting);

            if (queueEntry != null)
            {
                queuePosition = queueEntry.Position;
            }

            var now = DateTime.UtcNow;

            var confirmationRequired =
                booking.Status == ReservationStatus.Confirmed &&
                booking.ConfirmedAt == null &&
                now >= booking.Trip.ScheduledStart.AddMinutes(-30) &&
                now < booking.Trip.ScheduledStart.AddMinutes(-15);

            return new BookingDto
            {
                Id = booking.Id,
                TripId = booking.TripId,
                RouteName = booking.Trip.TransitRoute.RouteName,
                RouteCode = booking.Trip.TransitRoute.RouteCode,
                DepartureTime = booking.Trip.ScheduledStart,
                Status = booking.Status.ToString(),
                BoardingToken = booking.BoardingToken.ToString(),
                CreatedAt = booking.CreatedAt,
                QueuePosition = queuePosition,
                AttendanceConfirmed = booking.ConfirmedAt != null,
                ConfirmationRequired = confirmationRequired
            };
        }

        private int GetUserId()
        {
            return int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}