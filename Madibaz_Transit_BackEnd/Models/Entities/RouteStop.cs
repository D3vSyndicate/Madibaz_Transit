using System.ComponentModel.DataAnnotations.Schema;

namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class RouteStop
    {
        public int Id { get; set; }

        public int TransitRouteId { get; set; }

        public int BusStopId { get; set; }

        public int StopOrder { get; set; }

        [ForeignKey(nameof(TransitRouteId))]
        public TransitRoute TransitRoute { get; set; } = null!;

        [ForeignKey(nameof(BusStopId))]
        public BusStop BusStop { get; set; } = null!;
    }
}