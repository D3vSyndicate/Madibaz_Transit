// Models/Entities/AppUser.cs
// This is the LOGIN/ACCOUNT table — who can sign in, and what role
// they get. It's deliberately separate from Driver (which already
// exists and holds operational data like vehicle assignments) —
// later, a Driver record could link to an AppUser via a nullable
// DriverId FK, but that's not needed yet.

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public enum UserRole
    {
        Student,
        Driver,
        Marshal,
        Admin,
        ShuttleManager
    }

    public class AppUser
    {
        public int AppUserId { get; set; }

        // Only applies to NMU-issued accounts (students, and possibly
        // staff). Vendor accounts (Driver, ShuttleManager) won't have
        // one — leave it empty for those rather than forcing a fake value.
        public string? StudentNumber { get; set; }

        // e.g. "s256964895@mandela.ac.za"
        public string Email { get; set; } = string.Empty;

        // NEVER store the plain password — only ever the hash
        public string PasswordHash { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Student;

        public bool IsActive { get; set; } = true;

        // True right after a ShuttleManager (or Admin) creates this
        // account with a temp password. Blocks normal use until they
        // set their own password. False for self-registered students,
        // who set their password on signup and never hit this gate.
        public bool MustChangePassword { get; set; } = false;

        // Who provisioned this account — lets a ShuttleManager see/manage
        // only the drivers THEY created, not every driver in the system.
        // Null for students (self-registered) and for accounts Admin
        // created directly.
        public int? CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // [NotMapped] = "this is NOT a database column, don't create a
        // column for it." It exists on the C# class only, purely so
        // DbSeeder.cs can hold a readable plain-text password next to
        // each dummy user before hashing it. Never used outside seeding.
        [NotMapped]
        public string PlainPassword { get; set; } = string.Empty;
    }
}