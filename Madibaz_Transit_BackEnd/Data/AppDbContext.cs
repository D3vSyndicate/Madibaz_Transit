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

        public DbSet<BusLocation> BusLocations { get; set; }

        public DbSet<DriverShift> DriverShifts { get; set; }

        public DbSet<GPSCoordinateHistory> GPSCoordinateHistories { get; set; }

        public DbSet<Bus> Buses { get; set; }

        public DbSet<Driver> Drivers { get; set; }
        public DbSet<TransitRoute> TransitRoute {get; set; }
        public DbSet<BusStop> BusStops { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
        }
        
    }
}