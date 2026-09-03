using Madibaz_Transit_BackEnd.Data;
using Madibaz_Transit_BackEnd.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Madibaz_Transit_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class DriverController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        public DriverController(AppDbContext _dbContext)
        {
            this._dbContext = _dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDrivers()
        {
            var drivers = await _dbContext.Drivers.ToListAsync();
            return Ok(drivers);
        }

        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IActionResult> GetDriver(Guid id)
        {
            var driver = await _dbContext.Drivers.FindAsync(id);
            if(driver == null)
            {
                return NotFound();
            }
            return Ok(driver);
        }

       

        
        

    }
}
