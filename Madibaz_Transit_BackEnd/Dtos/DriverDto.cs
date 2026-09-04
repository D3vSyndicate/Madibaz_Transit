using Microsoft.Identity.Client;

namespace Madibaz_Transit_BackEnd.Dtos
{
    public class DriverDto
    {
        public Guid TripId { get; set; }
        public required string Status { get; set; }
        public required string Occupancy { get; set; } 
        
            
        

    }
}
