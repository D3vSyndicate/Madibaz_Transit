// Data/DbSeeder.cs
// Fills the database with dummy accounts so you can actually log in
// and test role-based access, without needing real university data.

using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Madibaz_Transit_BackEnd.Models.Entities;

namespace Madibaz_Transit_BackEnd.Data
{
    public static class DbSeeder
    {
        public static void SeedUsers(AppDbContext db)
        {
            if (db.Set<AppUser>().Any())
                return; // already seeded, don't duplicate on every restart

            var hasher = new PasswordHasher<AppUser>();

            // Realistic account SOURCES, not just realistic formatting:
            // - Students: real NMU student-number email format
            // - Admin/Marshal: NMU staff, firstname.surname@mandela.ac.za
            //   (these are ASSUMPTIONS — confirm with your team whether
            //   Marshals are NMU staff or student workers; if the latter,
            //   they'd actually use the student number format instead)
            // - Driver/ShuttleManager: the VENDOR's employees, not NMU's —
            //   they will never have an @mandela.ac.za address. In
            //   production, these accounts would be provisioned through
            //   a completely separate vendor onboarding process, which is
            //   exactly why they're not on the same login flow as students
            //   even though they use the same login FORM right now.
            var seedUsers = new[]
{
    new AppUser { StudentNumber = "s256964895", Email = "s256964895@mandela.ac.za", FullName = "Thabo Nkosi", Role = UserRole.Student, PlainPassword = "abcdef12" },
    new AppUser { StudentNumber = "s221345678", Email = "s221345678@mandela.ac.za", FullName = "Aisha Patel", Role = UserRole.Student, PlainPassword = "ghijkl34" },
    new AppUser { StudentNumber = "s224473131", Email = "s224473131@mandela.ac.za", FullName = "Sibusiso Nxumalo", Role = UserRole.Student, PlainPassword = "sibusiso" },
    new AppUser { StudentNumber = null, Email = "john.botha@quantumshuttle.co.za", FullName = "John Botha", Role = UserRole.Driver, PlainPassword = "mnopqr56" },
    new AppUser { StudentNumber = null, Email = "lindiwe.zulu@mandela.ac.za", FullName = "Lindiwe Zulu", Role = UserRole.Marshal, PlainPassword = "stuvwx78" },
    new AppUser { StudentNumber = null, Email = "admin.transport@mandela.ac.za", FullName = "You (Admin)", Role = UserRole.Admin, PlainPassword = "adminpw12" },
    new AppUser { StudentNumber = null, Email = "dispatch@quantumshuttle.co.za", FullName = "Vendor Dispatch", Role = UserRole.ShuttleManager, PlainPassword = "dispatch90" },
};

            foreach (var user in seedUsers)
            {
                user.PasswordHash = hasher.HashPassword(user, user.PlainPassword);
                user.IsActive = true;
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                db.Set<AppUser>().Add(user);
            }

            db.SaveChanges();
        }
    }
}

// NOTE: add this temporary helper property to AppUser.cs (or just hardcode
// the plain passwords directly in this file instead — either works, this
// version keeps them next to each user for readability):
//
//     [System.ComponentModel.DataAnnotations.Schema.NotMapped]
//     public string PlainPassword { get; set; } = string.Empty;
//
// [NotMapped] tells EF Core "this property is NOT a database column" —
// it exists on the C# class only, purely to make this seed file readable.