using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muzik67.Models
{
    public class GameProgress
    {
        [Key]
        public int ProgressId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int GameId { get; set; }

        public int Attempts { get; set; } = 1;

        public DateTime DateOfCompletion { get; set; } = DateTime.UtcNow;

        public int Score { get; set; } = 0;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("GameId")]
        public Game? Game { get; set; }
    }
}
