using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muzik67.Models;

public class UserBadge
{
    [Key]
    public int UserBadgeId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string BadgeCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string BadgeName { get; set; } = string.Empty;

    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
