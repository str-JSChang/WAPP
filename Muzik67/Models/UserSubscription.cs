using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muzik67.Models
{
    public class UserSubscription
    {
        [Key]
        public int SubscriptionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PlanId { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime EndDate { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // "Active", "Expired", "Cancelled"

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("PlanId")]
        public SubscriptionPlan? Plan { get; set; }
    }
}
