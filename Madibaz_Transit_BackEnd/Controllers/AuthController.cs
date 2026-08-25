// Controllers/AuthController.cs

using Madibaz_Transit_BackEnd.Data;
using Madibaz_Transit_BackEnd.Dtos;
using Madibaz_Transit_BackEnd.Models.Entities;
using Madibaz_Transit_BackEnd.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Madibaz_Transit_BackEnd.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtTokenService _jwtService;
        private readonly PasswordHasher<AppUser> _passwordHasher = new();

        public AuthController(AppDbContext db, JwtTokenService jwtService)
        {
            _db = db;
            _jwtService = jwtService;
        }

        // POST api/auth/login
        // Body: { "email": "s256964895@mandela.ac.za", "password": "..." }
        //
        // This is the "acts like SSO" login: student just types their
        // real-format university email + password, no role selection,
        // no signup form. Right now it checks against YOUR seeded data.
        // Later, if you get real access to the university's identity
        // system, this method is the ONLY thing that changes — swap
        // the password check for a real call to their system, keep
        // everything else (JWT issuance, role lookup) identical.
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto dto)
        {
            var user = await _db.Set<AppUser>()
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            // Deliberately vague error — "user not found" vs "wrong
            // password" should look identical to the client. Don't let
            // an attacker learn which emails exist by testing responses.
            if (user == null)
                return Unauthorized("Invalid email or password.");

            if (!user.IsActive)
                return Unauthorized("This account has been suspended. Contact an administrator.");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid email or password.");

            var token = _jwtService.GenerateToken(user);

            return Ok(new LoginResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                StudentNumber = user.StudentNumber,
                Role = user.Role.ToString(),
                MustChangePassword = user.MustChangePassword
            });
        }

        // POST api/auth/change-password
        // Requires a valid token (any logged-in user, any role) — this
        // is how a temp password gets replaced with one only the driver
        // knows, including the very first login where MustChangePassword
        // is still true.
        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
            var user = await _db.Set<AppUser>().FindAsync(userId);
            if (user == null) return NotFound();

            var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
            if (verify == PasswordVerificationResult.Failed)
                return Unauthorized("Current password is incorrect.");

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
            user.MustChangePassword = false; // gate lifted
            user.UpdatedAt = System.DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}