namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class ShuttleAssignment
    {
        public Guid AssignmentId { get; set; }
        public Guid ScheduleId { get; set; }
        public Schedule? Schedule { get; set; }
        public Guid BusId { get; set; }
        public Bus? Bus { get; set; }
        public Guid DriverId { get; set; }
        public Driver? Driver { get; set; }
        public Guid MarshalId { get; set; }
        public Marshal? Marshal { get; set; }
        public DateTime AssignmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}