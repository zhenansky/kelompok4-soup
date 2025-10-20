using System.ComponentModel.DataAnnotations;
using MyApp.WebAPI.Models;

namespace MyApp.WebAPI.DTO.Users
{
    public class UpdateUserRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public UserStatus Status { get; set; } = UserStatus.Active;
    }
}
