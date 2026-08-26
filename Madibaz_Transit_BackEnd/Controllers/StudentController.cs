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

        // Student profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var student = await _db.Set<AppUser>()
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

        // Student dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var now = DateTime.UtcNow;

            var activeBooking = await _db.Bookings
                .Include(b => b.ScheduledTrip)
                    .ThenInclude(t => t.TransitRoute)
                .Where(b =>
                    b.AppUserId == userId &&
                    (b.Status == "Confirmed" ||
                     b.Status == "Queued") &&
                    b.ScheduledTrip.DepartureTime >= now)
                .OrderBy(b => b.ScheduledTrip.DepartureTime)
                .Select(b => new
                {
                    BookingId = b.Id,
                    b.Status,
                    b.AttendanceConfirmed,
                    b.BoardingToken,

                    TripId = b.ScheduledTrip.Id,
                    b.ScheduledTrip.DepartureTime,

                    RouteName =
                        b.ScheduledTrip.TransitRoute.RouteName,

                    RouteCode =
                        b.ScheduledTrip.TransitRoute.RouteCode,

                    QueuePosition =
                        b.Status == "Queued"
                            ? _db.Bookings.Count(x =>
                                x.ScheduledTripId ==
                                    b.ScheduledTripId &&
                                x.Status == "Queued" &&
                                x.CreatedAt <= b.CreatedAt)
                            : 0
                })
                .FirstOrDefaultAsync();

            var upcomingTrips = await _db.ScheduledTrips
                .Include(t => t.TransitRoute)
                .Include(t => t.Bus)
                .Where(t =>
                    t.IsActive &&
                    t.DepartureTime >= now)
                .OrderBy(t => t.DepartureTime)
                .Take(5)
                .Select(t => new
                {
                    t.Id,
                    t.DepartureTime,
                    t.Status,

                    RouteName =
                        t.TransitRoute.RouteName,

                    RouteCode =
                        t.TransitRoute.RouteCode,

                    AvailableSeats =
                        t.Bus.Capacity -
                        _db.Bookings.Count(b =>
                            b.ScheduledTripId == t.Id &&
                            b.Status == "Confirmed")
                })
                .ToListAsync();

            return Ok(new
            {
                ActiveBooking = activeBooking,
                UpcomingTrips = upcomingTrips
            });
        }

        // Boarding pass
        [HttpGet("boarding-pass/{bookingId}")]
        public async Task<IActionResult> GetBoardingPass(
            int bookingId)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var booking = await _db.Bookings
                .Include(b => b.AppUser)
                .Include(b => b.ScheduledTrip)
                    .ThenInclude(t => t.TransitRoute)
                .Include(b => b.ScheduledTrip)
                    .ThenInclude(t => t.Bus)
                .FirstOrDefaultAsync(b =>
                    b.Id == bookingId &&
                    b.AppUserId == userId);

            if (booking == null)
                return NotFound("Booking not found.");

            if (booking.Status != "Confirmed")
            {
                return BadRequest(
                    "A boarding pass is only available for a confirmed booking."
                );
            }

            return Ok(new
            {
                BookingId = booking.Id,

                StudentName =
                    booking.AppUser.FullName,

                StudentNumber =
                    booking.AppUser.StudentNumber,

                RouteName =
                    booking.ScheduledTrip
                        .TransitRoute.RouteName,

                RouteCode =
                    booking.ScheduledTrip
                        .TransitRoute.RouteCode,

                DepartureTime =
                    booking.ScheduledTrip.DepartureTime,

                Bus = new
                {
                    booking.ScheduledTrip.Bus.FleetNumber,
                    booking.ScheduledTrip.Bus.RegistrationNumber
                },

                BoardingToken =
                    booking.BoardingToken,

                AttendanceConfirmed =
                    booking.AttendanceConfirmed,

                Status =
                    booking.Status
            });
        }
    }
}