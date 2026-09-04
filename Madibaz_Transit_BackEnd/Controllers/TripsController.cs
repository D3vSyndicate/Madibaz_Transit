using Madibaz_Transit_BackEnd.Data;
using Madibaz_Transit_BackEnd.Models.Entities;
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
            var trips = await _db.Trips
                
                .Where(t =>
              t.ScheduledStart >= DateTime.UtcNow)


                .OrderBy(t => t.ScheduledStart)
                .Select(t => new
                {
                    t.TripId,
                    t.ScheduledStart,
                    t.Status,

                    Route = new
                    {
                        t.TransitRoute.TransitRouteId,
                        t.TransitRoute.RouteName,
                        t.TransitRoute.RouteCode
                    },

                    Bus = new
                    {
                        t.Bus.BusId,
                        t.Bus.FleetNumber,
                        t.Bus.RegistrationNumber,
                        t.Bus.Capacity
                    },

                    BookedSeats = _db.SeatReservations.Count(r =>
                        r.TripId == t.TripId &&
                        (r.Status == ReservationStatus.Pending ||
                         r.Status == ReservationStatus.Confirmed ||
                         r.Status == ReservationStatus.Boarded)),

                    AvailableSeats =
                        t.Bus.Capacity -
                        _db.SeatReservations.Count(r =>
                            r.TripId == t.TripId &&
                            (r.Status == ReservationStatus.Pending ||
                             r.Status == ReservationStatus.Confirmed ||
                             r.Status == ReservationStatus.Boarded))
                })
                .ToListAsync();

            return Ok(trips);
        }
    }
}