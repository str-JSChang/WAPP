using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muzik67.Models
{
    public class SubscriptionPlan
    {
        [Key]
        public int PlanId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PlanName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [MaxLength(50)]
        public string Duration { get; set; } = "Monthly"; // "Monthly" or "Yearly"

        public string Description { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Status { get; set; } = "Active";
    }
}
