using System.ComponentModel.DataAnnotations;

namespace rnzTradingSim.Models
{
  public class Player
  {
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = "Player";
    public decimal Balance { get; set; } = 10000m;
    public decimal TotalWagered { get; set; } = 0m;
    public decimal TotalWon { get; set; } = 0m;
    public decimal TotalLost { get; set; } = 0m;
    public int GamesPlayed { get; set; } = 0;
    public int GamesWon { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public decimal BiggestWin { get; set; } = 0m;
    public decimal BiggestLoss { get; set; } = 0m;

    public decimal NetProfit => TotalWon - TotalLost;
    public double WinRate => GamesPlayed > 0 ? (double)GamesWon / GamesPlayed * 100 : 0;
    public decimal HouseEdge => TotalWagered > 0 ? (TotalLost - TotalWon) / TotalWagered * 100 : 0;
  }
}
