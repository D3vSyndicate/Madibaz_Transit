using Madibaz_Transit_BackEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Madibaz_Transit_BackEnd.Controllers
{
    [ApiController]
    [Route("api/trips")]
    [Authorize(Roles = "Student")]
    public class TripsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TripsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            var trips = await _db.ScheduledTrips
                .Where(t => t.IsActive)
                .OrderBy(t => t.DepartureTime)
                .Select(t => new
                {
                    t.Id,
                    t.DepartureTime,
                    t.Status,

                    Route = new
                    {
                        t.TransitRoute.Id,
                        t.TransitRoute.RouteName,
                        t.TransitRoute.RouteCode
                    },

                    Bus = new
                    {
                        t.Bus.Id,
                        t.Bus.FleetNumber,
                        t.Bus.RegistrationNumber,
                        t.Bus.Capacity
                    },

                    BookedSeats = _db.Bookings.Count(b =>
                        b.ScheduledTripId == t.Id &&
                        b.Status == "Confirmed"),

                    AvailableSeats = t.Bus.Capacity - _db.Bookings.Count(b =>
                        b.ScheduledTripId == t.Id &&
                        b.Status == "Confirmed")
                })
                .ToListAsync();

            return Ok(trips);
        }
    }
}