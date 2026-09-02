using Microsoft.Identity.Client;

namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class RouteStop
    {
        public Guid RouteStopId { get; set; }
        public Guid TransitRouteId { get; set; }
        public TransitRoute TransitRoute { get; set; }
        public Guid StopId { get; set; }
        public Stop Stop { get; set; }
        public required string Sequence {  get; set; }
        public required string ScheduledArrival { get; set; }
        public required string ScheduledDeparture { get; set; }

    }
}
