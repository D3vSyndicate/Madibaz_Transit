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

        // Create a booking
        [HttpPost]
        public async Task<IActionResult> CreateBooking(CreateBookingDto dto)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var trip = await _db.ScheduledTrips
                .Include(t => t.Bus)
                .Include(t => t.TransitRoute)
                .FirstOrDefaultAsync(t => t.Id == dto.ScheduledTripId);

            if (trip == null || !trip.IsActive)
                return NotFound("Scheduled trip not found.");

            // New bookings close 15 minutes before departure.
            if (trip.DepartureTime <= DateTime.UtcNow.AddMinutes(15))
            {
                return BadRequest(
                    "Bookings are closed because this shuttle departs in less than 15 minutes."
                );
            }

            // A student cannot have another active booking or
            // queue position for the same trip.
            var existingBooking = await _db.Bookings
                .AnyAsync(b =>
                    b.AppUserId == userId &&
                    b.ScheduledTripId == dto.ScheduledTripId &&
                    (b.Status == "Confirmed" || b.Status == "Queued"));

            if (existingBooking)
            {
                return Conflict(
                    "You already have a booking or queue position for this trip."
                );
            }

            var confirmedBookings = await _db.Bookings
                .CountAsync(b =>
                    b.ScheduledTripId == dto.ScheduledTripId &&
                    b.Status == "Confirmed");

            string status;
            string? boardingToken = null;

            // If there is space, confirm the booking.
            if (confirmedBookings < trip.Bus.Capacity)
            {
                status = "Confirmed";
                boardingToken = Guid.NewGuid().ToString("N");
            }
            // Otherwise place the student in the queue.
            else
            {
                status = "Queued";
            }

            var booking = new Booking
            {
                AppUserId = userId,
                ScheduledTripId = dto.ScheduledTripId,
                CreatedAt = DateTime.UtcNow,
                Status = status,
                BoardingToken = boardingToken,
                AttendanceConfirmed = false
            };

            _db.Bookings.Add(booking);

            await _db.SaveChangesAsync();

            return Ok(
                await BuildBookingDto(
                    booking.Id,
                    userId
                )
            );
        }

        // Get all bookings belonging to the logged-in student
        [HttpGet("my")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var bookings = await _db.Bookings
                .Where(b => b.AppUserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var result = new List<BookingDto>();

            foreach (var booking in bookings)
            {
                result.Add(
                    await BuildBookingDto(
                        booking.Id,
                        userId
                    )
                );
            }

            return Ok(result);
        }

        // Confirm whether the student is still coming.
        // Confirmation is available from 30 minutes
        // before departure until 15 minutes before departure.
        [HttpPost("{id}/confirm-attendance")]
        public async Task<IActionResult> ConfirmAttendance(
            int id,
            ConfirmAttendanceDto dto)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var booking = await _db.Bookings
                .Include(b => b.ScheduledTrip)
                .FirstOrDefaultAsync(b =>
                    b.Id == id &&
                    b.AppUserId == userId);

            if (booking == null)
                return NotFound("Booking not found.");

            if (booking.Status != "Confirmed")
            {
                return BadRequest(
                    "Only confirmed bookings can confirm attendance."
                );
            }

            var departureTime = booking.ScheduledTrip.DepartureTime;
            var now = DateTime.UtcNow;

            var confirmationStart =
                departureTime.AddMinutes(-30);

            var confirmationEnd =
                departureTime.AddMinutes(-15);

            if (now < confirmationStart)
            {
                return BadRequest(
                    "Attendance confirmation is not available yet."
                );
            }

            if (now >= confirmationEnd)
            {
                return BadRequest(
                    "The attendance confirmation period has ended."
                );
            }

            // Student says they are NOT coming.
            if (!dto.IsComing)
            {
                booking.Status = "Cancelled";
                booking.BoardingToken = null;
                booking.AttendanceConfirmed = false;

                await PromoteNextStudent(
                    booking.ScheduledTripId
                );

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    Message =
                        "Your booking has been cancelled."
                });
            }

            // Student confirms that they ARE coming.
            booking.AttendanceConfirmed = true;

            await _db.SaveChangesAsync();

            return Ok(
                await BuildBookingDto(
                    booking.Id,
                    userId
                )
            );
        }

        // Cancel a booking or queue position
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var booking = await _db.Bookings
                .FirstOrDefaultAsync(b =>
                    b.Id == id &&
                    b.AppUserId == userId);

            if (booking == null)
                return NotFound("Booking not found.");

            if (booking.Status == "Cancelled")
                return BadRequest(
                    "Booking is already cancelled."
                );

            if (booking.Status == "Expired")
                return BadRequest(
                    "This booking has already expired."
                );

            var wasConfirmed =
                booking.Status == "Confirmed";

            booking.Status = "Cancelled";
            booking.BoardingToken = null;
            booking.AttendanceConfirmed = false;

            // If a confirmed booking is cancelled,
            // give the seat to the next student in the queue.
            if (wasConfirmed)
            {
                await PromoteNextStudent(
                    booking.ScheduledTripId
                );
            }

            await _db.SaveChangesAsync();

            return NoContent();
        }

        // Get one booking belonging to the logged-in student
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var booking = await _db.Bookings
                .FirstOrDefaultAsync(b =>
                    b.Id == id &&
                    b.AppUserId == userId);

            if (booking == null)
                return NotFound("Booking not found.");

            return Ok(
                await BuildBookingDto(
                    booking.Id,
                    userId
                )
            );
        }

        // Automatically expire bookings that were not confirmed
        // by 15 minutes before departure.
        [HttpPost("process-expired")]
        public async Task<IActionResult> ProcessExpiredBookings()
        {
            var now = DateTime.UtcNow;

            var bookings = await _db.Bookings
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
                booking.AttendanceConfirmed = false;

                await PromoteNextStudent(
                    booking.ScheduledTripId
                );
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                ExpiredBookings = bookings.Count
            });
        }

        // Promote the first queued student when a confirmed
        // booking becomes available.
        private async Task PromoteNextStudent(
            int scheduledTripId)
        {
            var trip = await _db.ScheduledTrips
                .Include(t => t.Bus)
                .FirstOrDefaultAsync(
                    t => t.Id == scheduledTripId
                );

            if (trip == null)
                return;

            var confirmedCount = await _db.Bookings
                .CountAsync(b =>
                    b.ScheduledTripId == scheduledTripId &&
                    b.Status == "Confirmed");

            if (confirmedCount >= trip.Bus.Capacity)
                return;

            var nextInQueue = await _db.Bookings
                .Where(b =>
                    b.ScheduledTripId == scheduledTripId &&
                    b.Status == "Queued")
                .OrderBy(b => b.CreatedAt)
                .FirstOrDefaultAsync();

            if (nextInQueue == null)
                return;

            nextInQueue.Status = "Confirmed";

            nextInQueue.BoardingToken =
                Guid.NewGuid().ToString("N");

            nextInQueue.AttendanceConfirmed = false;
        }

        // Builds the booking response used by the mobile application.
        private async Task<BookingDto> BuildBookingDto(
            int bookingId,
            int userId)
        {
            var booking = await _db.Bookings
                .Include(b => b.ScheduledTrip)
                    .ThenInclude(t => t.TransitRoute)
                .FirstAsync(b =>
                    b.Id == bookingId &&
                    b.AppUserId == userId);

            var queuePosition =
                booking.Status == "Queued"
                    ? await _db.Bookings.CountAsync(b =>
                        b.ScheduledTripId ==
                            booking.ScheduledTripId &&
                        b.Status == "Queued" &&
                        b.CreatedAt <=
                            booking.CreatedAt)
                    : 0;

            var now = DateTime.UtcNow;

            // The student needs to confirm attendance
            // only during the 30-to-15-minute window.
            var confirmationRequired =
                booking.Status == "Confirmed" &&
                !booking.AttendanceConfirmed &&
                now >= booking.ScheduledTrip
                    .DepartureTime
                    .AddMinutes(-30) &&
                now < booking.ScheduledTrip
                    .DepartureTime
                    .AddMinutes(-15);

            return new BookingDto
            {
                Id = booking.Id,

                ScheduledTripId =
                    booking.ScheduledTripId,

                RouteName =
                    booking.ScheduledTrip
                        .TransitRoute
                        .RouteName,

                RouteCode =
                    booking.ScheduledTrip
                        .TransitRoute
                        .RouteCode,

                DepartureTime =
                    booking.ScheduledTrip
                        .DepartureTime,

                Status = booking.Status,

                BoardingToken =
                    booking.BoardingToken,

                CreatedAt =
                    booking.CreatedAt,

                QueuePosition =
                    queuePosition,

                AttendanceConfirmed =
                    booking.AttendanceConfirmed,

                ConfirmationRequired =
                    confirmationRequired
            };
        }
    }
}