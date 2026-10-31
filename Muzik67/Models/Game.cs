using System.ComponentModel.DataAnnotations;

namespace Muzik67.Models
{
    public class Game
    {
        [Key]
        public int GameId { get; set; }

        [Required]
        [MaxLength(100)]
        public string GameName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [MaxLength(20)]
        public string DifficultyLevel { get; set; } = "Easy"; // "Easy", "Medium", "Hard"

        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        public ICollection<GameQuiz> Quizzes { get; set; } = new List<GameQuiz>();
    }
}
