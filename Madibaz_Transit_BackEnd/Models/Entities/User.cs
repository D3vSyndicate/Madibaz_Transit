using Microsoft.Identity.Client;

namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class User
    {
        public Guid UserId { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
        public bool IsActive { get; set; }
        public bool MustChangePassword { get; set; }

    }
}
