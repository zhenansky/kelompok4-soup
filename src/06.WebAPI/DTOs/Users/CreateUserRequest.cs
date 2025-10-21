using System.ComponentModel.DataAnnotations;
using MyApp.WebAPI.Models;

namespace MyApp.WebAPI.DTO.Users
{
    public class CreateUserRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = "User123!";

        public string Role { get; set; } = "User";

        public UserStatus Status { get; set; } = UserStatus.Active;
    }
}
