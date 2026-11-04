using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muzik67.Models
{
    public class GameQuiz
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public int GameId { get; set; }

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        // Comma or JSON-formatted options, e.g. "C (Do), D (Re), E (Mi), F (Fa)"
        [Required]
        public string AnswerOptions { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string CorrectAnswer { get; set; } = string.Empty;

        [MaxLength(20)]
        public string ActivityType { get; set; } = "MultipleChoice";

        [MaxLength(500)]
        public string MediaUrl { get; set; } = string.Empty;

        public string OptionImageUrls { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        [ForeignKey("GameId")]
        public Game? Game { get; set; }
    }
}
