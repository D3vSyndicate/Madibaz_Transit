namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class PickupStop
    {
        public Guid StopId { get; set; }

        public Guid RouteId { get; set; }

        public string StopName { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public int StopOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}