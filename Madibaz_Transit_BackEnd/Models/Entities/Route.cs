namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class Route
    {
        public Guid RouteId { get; set; }

        public string RouteName { get; set; } = string.Empty;

        public string Origin { get; set; } = string.Empty;

        public string Destination { get; set; } = string.Empty;

        public decimal Distance { get; set; }

        public string Status { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}