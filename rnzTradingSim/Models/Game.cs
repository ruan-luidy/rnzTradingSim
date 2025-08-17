using System.ComponentModel.DataAnnotations;

namespace rnzTradingSim.Models
{
  public enum GameType
  {
    Coinflip,
    Dice,
    Mines,
    Slots,
    Trading
  }

  public enum GameStatus
  {
    InProgress,
    Won,
    Lost,
    Cancelled
  }

  public class Game
  {
    [Key]
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public GameType Type { get; set; }
    public GameStatus Status { get; set; } = GameStatus.InProgress;
    public decimal BetAmount { get; set; }
    public decimal CurrentPayout { get; set; } = 0m;
    public decimal Multiplier { get; set; } = 1m;
    public DateTime StartedAt { get; set; } = DateTime.Now;
    public DateTime? FinishedAt { get; set; }
    public string GameData { get; set; } = "{}"; // JSON data specific to game type
    public bool IsActive => Status == GameStatus.InProgress;
    public TimeSpan Duration => FinishedAt?.Subtract(StartedAt) ?? DateTime.Now.Subtract(StartedAt);

    public Player? Player { get; set; }

    public void FinishGame(GameStatus status, decimal finalPayout = 0m)
    {
      Status = status;
      CurrentPayout = finalPayout;
      FinishedAt = DateTime.Now;
    }

    public GameResult ToGameResult()
    {
      return new GameResult
      {
        PlayerId = PlayerId,
        GameType = Type.ToString(),
        BetAmount = BetAmount,
        WinAmount = Status == GameStatus.Won ? CurrentPayout : 0m,
        IsWin = Status == GameStatus.Won,
        PlayedAt = FinishedAt ?? DateTime.Now,
        Multiplier = Multiplier,
        Details = GameData
      };
    }
  }
}
