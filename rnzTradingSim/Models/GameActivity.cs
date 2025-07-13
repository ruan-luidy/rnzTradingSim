namespace rnzTradingSim.Models
{
  public class GameActivity
  {
    public string Game { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsWin { get; set; }
  }
}