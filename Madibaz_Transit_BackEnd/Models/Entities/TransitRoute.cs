
namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class TransitRoute
    {
        public Guid TransitRouteId { get; set; }

        public string RouteName { get; set; } = string.Empty;

        public string RouteCode { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<BusStop> BusStops { get; set; } = new List<BusStop>();
    }
}