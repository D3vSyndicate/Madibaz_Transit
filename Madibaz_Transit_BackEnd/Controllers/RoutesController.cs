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
            var routes = await _db.TransitRoutes
                .Where(r => r.IsActive)
                .Select(r => new
                {
                    r.TransitRouteId,
                    r.RouteName,
                    r.RouteCode,
                    r.Description,

                    Stops = r.BusStops
                        .OrderBy(s => s.StopOrder)
                        .Select(s => new
                        {
                            s.Id,
                            s.StopOrder,
                            StopName = s.StopName,
                            s.Latitude,
                            s.Longitude
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(routes);
        }
    }
}