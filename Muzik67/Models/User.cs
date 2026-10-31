using System.ComponentModel.DataAnnotations;

namespace Muzik67.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "Member"; // "Admin" or "Member"

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string AccountStatus { get; set; } = "Active"; // "Active", "Suspended"
    }
}
