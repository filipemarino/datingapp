using System.ComponentModel.DataAnnotations;

namespace DatingApp.WebAPI.DTO
{
    public class UserForLoginDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}