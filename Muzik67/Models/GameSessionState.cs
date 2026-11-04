namespace Muzik67.Models;

public sealed class GameSessionState
{
    public int GameId { get; set; }
    public int QuestionIndex { get; set; }
    public int Score { get; set; }
    public int Lives { get; set; } = 3;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}
