namespace rnzTradingSim.Models;

public class GameResult
{
  public string GameType { get; set; } = string.Empty;
  public decimal BetAmount { get; set; }
  public decimal WinAmount { get; set; }
  public bool IsWin { get; set; }
  public DateTime PlayedAt { get; set; } = DateTime.Now;
  public string Details { get; set; } = string.Empty; // JSON with game-specific data
  public decimal Multiplier { get; set; } = 0m;

  public decimal NetResult => IsWin ? WinAmount - BetAmount : -BetAmount;
}