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

        [ForeignKey("GameId")]
        public Game? Game { get; set; }
    }
}
