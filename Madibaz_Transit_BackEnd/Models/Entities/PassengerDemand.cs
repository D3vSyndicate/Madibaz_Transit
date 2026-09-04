namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class PassengerDemand
    {
        public Guid DemandID { get; set; }
        public Guid ScheduleID { get; set; }
        public Schedule? Schedule { get; set; }
        public DateTime Date { get; set; }
        public int ExpectedPassengers { get; set; }
        public int TotalBooked { get; set; }
        public int TotalBoarded { get; set; }
        public int NoShowCount { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}