// Services/JwtTokenService.cs
// Issues YOUR app's JWT once a login (email + password) has been
// verified. This token is what [Authorize(Roles = "...")] checks on
// every other request from here on.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Madibaz_Transit_BackEnd.Models.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Madibaz_Transit_BackEnd.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(AppUser user)
        {
            var signingKey = _config["AppJwt:SigningKey"]!;
            var issuer = _config["AppJwt:Issuer"]!;
            var audience = _config["AppJwt:Audience"]!;
            var expiryMinutes = int.Parse(_config["AppJwt:ExpiryMinutes"] ?? "60");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.AppUserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim("studentNumber", user.StudentNumber ?? string.Empty),
                // THIS claim is what [Authorize(Roles = "Admin")] checks.
                // It came from YOUR database — the user never set it.
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}