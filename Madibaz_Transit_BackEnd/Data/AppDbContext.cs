using Madibaz_Transit_BackEnd.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Madibaz_Transit_BackEnd.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // =====================================================
        // USERS
        // =====================================================

        public DbSet<AppUser> Users { get; set; }
        public DbSet<StudentProfiles> StudentProfiles { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Marshal> Marshals { get; set; }

        // =====================================================
        // TRANSPORT
        // =====================================================

        public DbSet<Bus> Buses { get; set; }
        public DbSet<TransitRoute> TransitRoutes { get; set; }
        public DbSet<BusStop> BusStops { get; set; }

        // =====================================================
        // SCHEDULES / SHIFTS / ASSIGNMENTS
        // =====================================================

        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<DriverShift> DriverShifts { get; set; }
        public DbSet<ShuttleAssignment> ShuttleAssignments { get; set; }

        // =====================================================
        // TRIPS
        // =====================================================

        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripStatusHistory> TripStatusHistories { get; set; }

        // =====================================================
        // BOOKINGS / QUEUES
        // =====================================================

        public DbSet<SeatReservations> SeatReservations { get; set; }
        public DbSet<ActiveQueues> ActiveQueues { get; set; }

        // =====================================================
        // LOCATION
        // =====================================================

        public DbSet<BusLocation> BusLocations { get; set; }
        public DbSet<GPSCoordinateHistory> GPSCoordinateHistories { get; set; }

        // =====================================================
        // COMMUNICATION
        // =====================================================

        public DbSet<Announcements> Announcements { get; set; }
        public DbSet<Notifications> Notifications { get; set; }

        // =====================================================
        // STUDENT SERVICES
        // =====================================================

        public DbSet<ComplainTickets> ComplainTickets { get; set; }
        public DbSet<LostPropertyTickets> LostPropertyTickets { get; set; }
        public DbSet<ShuttleRequests> ShuttleRequests { get; set; }

        // =====================================================
        // OPERATIONS
        // =====================================================

        public DbSet<Incident> Incidents { get; set; }
        public DbSet<DelayReport> DelayReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =====================================================
            // PRIMARY KEYS
            // =====================================================

            modelBuilder.Entity<AppUser>()
                .HasKey(x => x.AppUserId);

            modelBuilder.Entity<StudentProfiles>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Bus>()
                .HasKey(x => x.BusId);

            modelBuilder.Entity<Driver>()
                .HasKey(x => x.DriverId);

            modelBuilder.Entity<Marshal>()
                .HasKey(x => x.MarshalId);

            modelBuilder.Entity<TransitRoute>()
                .HasKey(x => x.TransitRouteId);

            modelBuilder.Entity<BusStop>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Schedule>()
                .HasKey(x => x.ScheduleId);

            modelBuilder.Entity<DriverShift>()
                .HasKey(x => x.DriverShiftId);

            modelBuilder.Entity<ShuttleAssignment>()
                .HasKey(x => x.AssignmentId);

            modelBuilder.Entity<Trip>()
                .HasKey(x => x.TripId);

            modelBuilder.Entity<TripStatusHistory>()
                .HasKey(x => x.HistoryId);

            modelBuilder.Entity<SeatReservations>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<ActiveQueues>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<BusLocation>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<GPSCoordinateHistory>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Announcements>()
                .HasKey(x => x.AnnouncementId);

            modelBuilder.Entity<Notifications>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<ComplainTickets>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<LostPropertyTickets>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<ShuttleRequests>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Incident>()
                .HasKey(x => x.IncidentId);

            modelBuilder.Entity<DelayReport>()
                .HasKey(x => x.DelayId);


            // =====================================================
            // DECIMAL PRECISION
            // =====================================================

            modelBuilder.Entity<BusLocation>()
                .Property(x => x.Latitude)
                .HasPrecision(10, 7);

            modelBuilder.Entity<BusLocation>()
                .Property(x => x.Longitude)
                .HasPrecision(10, 7);

            modelBuilder.Entity<BusLocation>()
                .Property(x => x.Heading)
                .HasPrecision(6, 2);

            modelBuilder.Entity<GPSCoordinateHistory>()
                .Property(x => x.Latitude)
                .HasPrecision(10, 7);

            modelBuilder.Entity<GPSCoordinateHistory>()
                .Property(x => x.Longitude)
                .HasPrecision(10, 7);

            modelBuilder.Entity<BusStop>()
                .Property(x => x.Latitude)
                .HasPrecision(10, 7);

            modelBuilder.Entity<BusStop>()
                .Property(x => x.Longitude)
                .HasPrecision(10, 7);


            // =====================================================
            // APP USER -> CREATED BY USER
            // =====================================================

            modelBuilder.Entity<AppUser>()
                .HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // STUDENT PROFILE -> APP USER
            // =====================================================

            modelBuilder.Entity<StudentProfiles>()
                .HasOne(x => x.AppUser)
                .WithMany(x => x.StudentProfiles)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);


            // =====================================================
            // DRIVER -> APP USER
            // =====================================================

            modelBuilder.Entity<Driver>()
                .HasOne(x => x.AppUser)
                .WithMany(x => x.Drivers)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // MARSHAL -> APP USER
            // =====================================================

            modelBuilder.Entity<Marshal>()
                .HasOne(x => x.User)
                .WithMany(x => x.Marshals)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // ROUTE -> BUS STOPS
            // =====================================================

            modelBuilder.Entity<BusStop>()
                .HasOne(x => x.TransitRoute)
                .WithMany(x => x.BusStops)
                .HasForeignKey(x => x.TransitRouteId)
                .OnDelete(DeleteBehavior.Cascade);


            // =====================================================
            // DRIVER SHIFT -> DRIVER
            // =====================================================

            modelBuilder.Entity<DriverShift>()
                .HasOne(x => x.Driver)
                .WithMany(x => x.DriverShifts)
                .HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // DRIVER SHIFT -> BUS
            // =====================================================

            modelBuilder.Entity<DriverShift>()
                .HasOne(x => x.Bus)
                .WithMany(x => x.DriverShifts)
                .HasForeignKey(x => x.BusId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // DRIVER SHIFT -> ROUTE
            // =====================================================

            modelBuilder.Entity<DriverShift>()
                .HasOne(x => x.Route)
                .WithMany()
                .HasForeignKey(x => x.RouteId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // DRIVER SHIFT -> SCHEDULE
            // =====================================================

            modelBuilder.Entity<DriverShift>()
                .HasOne(x => x.Schedule)
                .WithMany()
                .HasForeignKey(x => x.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // TRIP -> DRIVER SHIFT
            // =====================================================

            modelBuilder.Entity<Trip>()
                .HasOne(x => x.DriverShift)
                .WithMany()
                .HasForeignKey(x => x.DriverShiftId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // TRIP -> TRANSIT ROUTE
            // =====================================================

            modelBuilder.Entity<Trip>()
                .HasOne(x => x.TransitRoute)
                .WithMany()
                .HasForeignKey(x => x.TransitRouteId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // TRIP -> BUS
            // =====================================================

            modelBuilder.Entity<Trip>()
                .HasOne(x => x.Bus)
                .WithMany(x => x.Trips)
                .HasForeignKey(x => x.BusId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // SHUTTLE ASSIGNMENT -> SCHEDULE
            // =====================================================

            modelBuilder.Entity<ShuttleAssignment>()
                .HasOne(x => x.Schedule)
                .WithMany()
                .HasForeignKey(x => x.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // SHUTTLE ASSIGNMENT -> BUS
            // =====================================================

            modelBuilder.Entity<ShuttleAssignment>()
                .HasOne(x => x.Bus)
                .WithMany(x => x.ShuttleAssignments)
                .HasForeignKey(x => x.BusId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // SHUTTLE ASSIGNMENT -> DRIVER
            // =====================================================

            modelBuilder.Entity<ShuttleAssignment>()
                .HasOne(x => x.Driver)
                .WithMany()
                .HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // SHUTTLE ASSIGNMENT -> MARSHAL
            // =====================================================

            modelBuilder.Entity<ShuttleAssignment>()
                .HasOne(x => x.Marshal)
                .WithMany()
                .HasForeignKey(x => x.MarshalId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // SEAT RESERVATION -> APP USER
            // =====================================================

            modelBuilder.Entity<SeatReservations>()
                .HasOne(x => x.AppUser)
                .WithMany(x => x.SeatReservations)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // SEAT RESERVATION -> TRIP
            // =====================================================

            modelBuilder.Entity<SeatReservations>()
                .HasOne(x => x.Trip)
                .WithMany(x => x.SeatReservations)
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.Cascade);


            // =====================================================
            // ACTIVE QUEUE -> APP USER
            // =====================================================

            modelBuilder.Entity<ActiveQueues>()
                .HasOne(x => x.AppUser)
                .WithMany(x => x.ActiveQueues)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // ACTIVE QUEUE -> TRIP
            // =====================================================

            modelBuilder.Entity<ActiveQueues>()
                .HasOne(x => x.Trip)
                .WithMany(x => x.ActiveQueues)
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.Cascade);


            // =====================================================
            // TRIP STATUS HISTORY -> TRIP
            // =====================================================

            modelBuilder.Entity<TripStatusHistory>()
                .HasOne(x => x.Trip)
                .WithMany(x => x.StatusHistory)
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.Cascade);
       
            
            
            // =====================================================
            // COMPLAINT -> APP USER
            // =====================================================

            modelBuilder.Entity<ComplainTickets>()
                .HasOne(x => x.AppUser)
                .WithMany(x => x.ComplaintTickets)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // COMPLAINT -> TRIP
            // =====================================================

            modelBuilder.Entity<ComplainTickets>()
                .HasOne(x => x.Trip)
                .WithMany()
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.SetNull);


            // =====================================================
            // LOST PROPERTY -> APP USER
            // =====================================================

            modelBuilder.Entity<LostPropertyTickets>()
                .HasOne(x => x.AppUser)
                .WithMany()
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // LOST PROPERTY -> TRIP
            // =====================================================

            modelBuilder.Entity<LostPropertyTickets>()
                .HasOne(x => x.Trip)
                .WithMany()
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.SetNull);


            // =====================================================
            // SHUTTLE REQUEST -> APP USER
            // =====================================================

            modelBuilder.Entity<ShuttleRequests>()
                .HasOne(x => x.AppUser)
                .WithMany()
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);


            // =====================================================
            // NOTIFICATION -> APP USER
            // =====================================================

            modelBuilder.Entity<Notifications>()
                .HasOne(x => x.AppUser)
                .WithMany()
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);


            // =====================================================
            // INCIDENT -> BUS
            // =====================================================

            modelBuilder.Entity<Incident>()
                .HasOne(x => x.Bus)
                .WithMany(x => x.Incidents)
                .HasForeignKey(x => x.BusId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // INCIDENT -> TRIP
            // =====================================================

            modelBuilder.Entity<Incident>()
                .HasOne(x => x.Trip)
                .WithMany()
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // DELAY REPORT -> TRIP
            // =====================================================

            modelBuilder.Entity<DelayReport>()
                .HasOne(x => x.Trip)
                .WithMany()
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // DELAY REPORT -> DRIVER
            // =====================================================

            modelBuilder.Entity<DelayReport>()
                .HasOne(x => x.Driver)
                .WithMany()
                .HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}