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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BusLocation>()
                .Property(b => b.Latitude)
                .HasPrecision(10, 7);

            modelBuilder.Entity<BusLocation>()
                .Property(b => b.Longitude)
                .HasPrecision(10, 7);

            modelBuilder.Entity<BusLocation>()
                .Property(b => b.Heading)
                .HasPrecision(10, 2);

            modelBuilder.Entity<GPSCoordinateHistory>()
                .Property(g => g.Latitude)
                .HasPrecision(10, 7);

            modelBuilder.Entity<GPSCoordinateHistory>()
                .Property(g => g.Longitude)
                .HasPrecision(10, 7);
        }
    }
}