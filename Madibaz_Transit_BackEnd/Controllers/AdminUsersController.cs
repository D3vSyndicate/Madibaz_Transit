// Controllers/AdminUsersController.cs
// Owner: Admin
//
// Right now, this does exactly ONE thing: create a ShuttleManager
// account, the same way ShuttleManagerController creates Driver
// accounts — generated temp password, forced change on first login.
// This closes the gap where DbSeeder was the only way a ShuttleManager
// account came into existence, with a fixed password and no forced
// change, which didn't match the real pattern.
//
// Admin does NOT have password reset capability over any account —
// that's self-service now via /api/auth/forgot-password, for everyone.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Madibaz_Transit_BackEnd.Data;
using Madibaz_Transit_BackEnd.Dtos;
using Madibaz_Transit_BackEnd.Models.Entities;
using Madibaz_Transit_BackEnd.Services;

namespace Madibaz_Transit_BackEnd.Controllers
{
    [ApiController]
    [Route("api/admin/shuttle-managers")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly PasswordHasher<AppUser> _passwordHasher = new();

        public AdminUsersController(AppDbContext db)
        {
            _db = db;
        }

        // POST api/admin/shuttle-managers
        // Body: { "email": "...", "fullName": "..." }
        // Same shape as ShuttleManagerController's CreateDriver —
        // deliberately consistent pattern across the whole system.
        [HttpPost]
        public async Task<ActionResult<CreateDriverResponseDto>> CreateShuttleManager(CreateDriverRequestDto dto)
        {
            bool emailExists = await _db.Set<AppUser>().AnyAsync(u => u.Email == dto.Email);
            if (emailExists)
                return Conflict("An account with this email already exists.");

            var tempPassword = TempPasswordGenerator.Generate();

            var manager = new AppUser
            {
                Email = dto.Email,
                FullName = dto.FullName,
                Role = UserRole.ShuttleManager,
                IsActive = true,
                MustChangePassword = true, // same forced-change pattern as Driver
                StudentNumber = null
                // CreatedByUserId intentionally left null — this account
                // isn't "owned" by anyone the way a Driver is owned by
                // the ShuttleManager who created them.
            };
            manager.PasswordHash = _passwordHasher.HashPassword(manager, tempPassword);

            _db.Set<AppUser>().Add(manager);
            await _db.SaveChangesAsync();

            return Ok(new CreateDriverResponseDto
            {
                AppUserId = manager.AppUserId,
                Email = manager.Email,
                FullName = manager.FullName,
                TemporaryPassword = tempPassword // shown once, relayed out-of-system to the vendor
            });
        }
    }
}