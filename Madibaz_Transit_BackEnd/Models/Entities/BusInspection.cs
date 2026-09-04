namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class BusInspection
    {
        public Guid BusInspectionId { get; set; }

        public int BusId { get; set; }
        public Bus Bus { get; set; } = null!;

        public int InspectedByUserId { get; set; }
        public AppUser InspectedByUser { get; set; } = null!;

        public DateTime InspectionDate { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Passed";

        public string? Notes { get; set; }

        public bool IsRoadworthy { get; set; } = true;
    }
}
