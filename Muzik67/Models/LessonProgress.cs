using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muzik67.Models;

public class LessonProgress
{
    [Key]
    public int LessonProgressId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int ContentId { get; set; }

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(ContentId))]
    public LearningContent? Content { get; set; }
}
