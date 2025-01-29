using System.ComponentModel.DataAnnotations;

namespace NewsSite.Server.Models.Auth
{
    public class RegisterRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string Language { get; set; } = "en";
    }
} 