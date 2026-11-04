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

        public string BodyContent { get; set; } = string.Empty;

        [MaxLength(500)]
        public string MediaUrl { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ContentType { get; set; } = "Theory"; // "Theory", "EarTraining", "Instrument", "Song"

        [MaxLength(20)]
        public string DifficultyLevel { get; set; } = "Easy"; // "Easy", "Medium", "Hard"

        [MaxLength(50)]
        public string CreatedBy { get; set; } = "Admin";

        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        public int SortOrder { get; set; }

        [MaxLength(20)]
        public string ApprovalStatus { get; set; } = "Approved";

        [MaxLength(50)]
        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
