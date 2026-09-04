using System.Security.Claims;
using Madibaz_Transit_BackEnd.Data;
using Madibaz_Transit_BackEnd.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Madibaz_Transit_BackEnd.Controllers
{
    [ApiController]
    [Route("api/student")]
    [Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {
        private readonly AppDbContext _db;

        public StudentController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();

            var student = await _db.Users
                .Where(u => u.AppUserId == userId)
                .Select(u => new
                {
                    u.AppUserId,
                    u.FullName,
                    u.Email,
                    u.StudentNumber,
                    Role = u.Role.ToString()
                })
                .FirstOrDefaultAsync();

            if (student == null)
                return NotFound("Student account not found.");

            return Ok(student);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = GetUserId();
            var now = DateTime.UtcNow;

            var activeBooking = await _db.SeatReservations
                .Include(r => r.Trip)
                    .ThenInclude(t => t.TransitRoute)
                .Where(r =>
                    r.AppUserId == userId &&
                    (r.Status == ReservationStatus.Pending ||
                     r.Status == ReservationStatus.Confirmed ||
                     r.Status == ReservationStatus.Boarded) &&
                    r.Trip.ScheduledStart >= now)
                .OrderBy(r => r.Trip.ScheduledStart)
                .Select(r => new
                {
                    BookingId = r.Id,
                    Status = r.Status.ToString(),
                    AttendanceConfirmed = r.ConfirmedAt != null,
                    BoardingToken = r.BoardingToken,
                    TripId = r.Trip.TripId,
                    DepartureTime = r.Trip.ScheduledStart,
                    RouteName = r.Trip.TransitRoute.RouteName,
                    RouteCode = r.Trip.TransitRoute.RouteCode,
                    QueuePosition = 0,
                    ConfirmationRequired =
                        r.Status == ReservationStatus.Confirmed &&
                        r.ConfirmedAt == null &&
                        now >= r.Trip.ScheduledStart.AddMinutes(-30) &&
                        now < r.Trip.ScheduledStart.AddMinutes(-15)
                })
                .FirstOrDefaultAsync();

            object? dashboardBooking = activeBooking;

            if (dashboardBooking == null)
            {
                dashboardBooking = await _db.ActiveQueues
                    .Include(q => q.Trip)
                        .ThenInclude(t => t.TransitRoute)
                    .Where(q =>
                        q.AppUserId == userId &&
                        q.Status == QueueEntryStatus.Waiting &&
                        q.Trip.ScheduledStart >= now)
                    .OrderBy(q => q.Trip.ScheduledStart)
                    .Select(q => new
                    {
                        BookingId = q.Id,
                        Status = "Queued",
                        AttendanceConfirmed = false,
                        BoardingToken = Guid.Empty,
                        TripId = q.Trip.TripId,
                        DepartureTime = q.Trip.ScheduledStart,
                        RouteName = q.Trip.TransitRoute.RouteName,
                        RouteCode = q.Trip.TransitRoute.RouteCode,
                        QueuePosition = q.Position,
                        ConfirmationRequired = false
                    })
                    .FirstOrDefaultAsync();
            }

            var upcomingTrips = await _db.Trips
                .Include(t => t.TransitRoute)
                .Include(t => t.Bus)
                .Where(t =>
                    t.ScheduledStart >= now)
                .OrderBy(t => t.ScheduledStart)
                .Take(5)
                .Select(t => new
                {
                    t.TripId,
                    DepartureTime = t.ScheduledStart,
                    t.Status,
                    RouteName = t.TransitRoute.RouteName,
                    RouteCode = t.TransitRoute.RouteCode,
                    AvailableSeats =
                        t.Bus.Capacity -
                        _db.SeatReservations.Count(r =>
                            r.TripId == t.TripId &&
                            (r.Status == ReservationStatus.Pending ||
                             r.Status == ReservationStatus.Confirmed ||
                             r.Status == ReservationStatus.Boarded))
                })
                .ToListAsync();

            return Ok(new
            {
                ActiveBooking = dashboardBooking,
                UpcomingTrips = upcomingTrips
            });
        }

        [HttpGet("boarding-pass/{bookingId}")]
        public async Task<IActionResult> GetBoardingPass(Guid bookingId)
        {
            var userId = GetUserId();

            var booking = await _db.SeatReservations
                .Include(r => r.AppUser)
                .Include(r => r.Trip)
                    .ThenInclude(t => t.TransitRoute)
                .Include(r => r.Trip)
                    .ThenInclude(t => t.Bus)
                .FirstOrDefaultAsync(r =>
                    r.Id == bookingId &&
                    r.AppUserId == userId);

            if (booking == null)
                return NotFound("Booking not found.");

            if (booking.Status != ReservationStatus.Confirmed)
            {
                return BadRequest(
                    "A boarding pass is only available for a confirmed booking.");
            }

            return Ok(new
            {
                BookingId = booking.Id,
                StudentName = booking.AppUser.FullName,
                StudentNumber = booking.AppUser.StudentNumber,
                RouteName = booking.Trip.TransitRoute.RouteName,
                RouteCode = booking.Trip.TransitRoute.RouteCode,
                DepartureTime = booking.Trip.ScheduledStart,

                Bus = new
                {
                    booking.Trip.Bus.FleetNumber,
                    booking.Trip.Bus.RegistrationNumber
                },

                BoardingToken = booking.BoardingToken,
                AttendanceConfirmed = booking.ConfirmedAt != null,
                Status = booking.Status.ToString()
            });
        }

        private int GetUserId()
        {
            return int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}
