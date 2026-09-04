
namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class Bus
    {
        public int BusId { get; set; }

        public string RegistrationNumber { get; set; } = string.Empty;

        public string FleetNumber { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public string Status { get; set; } = "Available";

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<BusLocation> BusLocations { get; set; }
            = new List<BusLocation>();

        public ICollection<GPSCoordinateHistory> GPSCoordinateHistories { get; set; }
            = new List<GPSCoordinateHistory>();

        public ICollection<DriverShift> DriverShifts { get; set; }
            = new List<DriverShift>();

        public ICollection<Trip> Trips { get; set; }
            = new List<Trip>();

        public ICollection<ShuttleAssignment> ShuttleAssignments { get; set; }
            = new List<ShuttleAssignment>();

        public ICollection<Incident> Incidents { get; set; }
            = new List<Incident>();

        public ICollection<BusInspection> BusInspections { get; set; }
            = new List<BusInspection>();
    }
}