using System.ComponentModel.DataAnnotations;

namespace Muzik67.Models
{
    public class LearningContent
    {
        [Key]
        public int ContentId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ContentType { get; set; } = "Theory"; // "Theory", "EarTraining", "Instrument"

        [MaxLength(20)]
        public string DifficultyLevel { get; set; } = "Easy"; // "Easy", "Medium", "Hard"

        [MaxLength(50)]
        public string CreatedBy { get; set; } = "Admin";

        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
