using Madibaz_Transit_BackEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Madibaz_Transit_BackEnd.Controllers
{
    [ApiController]
    [Route("api/routes")]
    [Authorize(Roles = "Student")]
    public class RoutesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public RoutesController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoutes()
        {
            var routes = await _db.TransitRoute
                .Where(r => r.IsActive)
                .Select(r => new
                {
                    r.Id,
                    r.RouteName,
                    r.RouteCode,
                    r.Description,

                    Stops = _db.RouteStops
                        .Where(rs => rs.TransitRouteId == r.Id)
                        .OrderBy(rs => rs.StopOrder)
                        .Select(rs => new
                        {
                            rs.BusStopId,
                            rs.StopOrder,
                            StopName = rs.BusStop.StopName,
                            rs.BusStop.Latitude,
                            rs.BusStop.Longitude
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(routes);
        }
    }
}