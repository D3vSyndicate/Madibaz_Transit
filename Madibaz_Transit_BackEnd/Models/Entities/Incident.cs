namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class Incident
    {
        public Guid IncidentId { get; set; }
        public Guid TripId { get; set; }
        public Guid BusId { get; set; }
        public Bus Bus { get; set; }
        public Trip Trip { get; set; }
        public  IncidentType Type { get; set; }
        public string Description { get; set; }
        public Severity Severity { get; set; }
        public string Status { get; set; }
        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
        public DateTime ResolvedAt { get; set; } 

       
    }

    public enum IncidentType
    {
        VEHICLE_FAILURE,
        ACCIDENT,
        MEDICAL,
        SECURITY,
        ROAD_BLOCKADE,
        OTHER

    }

    public enum Severity
    {
        LOW,
        MEDIUM,
        HIGH,
        CRITICAL
    }
}
