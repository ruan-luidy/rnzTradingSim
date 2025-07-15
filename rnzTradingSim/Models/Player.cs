using System.ComponentModel.DataAnnotations;
using static GameConstants;

namespace rnzTradingSim.Models
{
  public class Player
  {
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = "Player";
    public decimal Balance { get; set; } = DEFAULT_BALANCE;
    public decimal TotalWagered { get; set; } = 0m;
    public decimal TotalWon { get; set; } = 0m;
    public decimal TotalLost { get; set; } = 0m;
    public int GamesPlayed { get; set; } = 0;
    public int GamesWon { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public decimal BiggestWin { get; set; } = 0m;
    public decimal BiggestLoss { get; set; } = 0m;

    public decimal NetProfit => TotalWon - TotalLost;
    public decimal WinRate => GamesPlayed > 0 ? (decimal)GamesWon / GamesPlayed * 100 : 0;
    public decimal HouseEdge => TotalWagered > 0 ? (TotalLost - TotalWon) / TotalWagered * 100 : 0;
  }
}