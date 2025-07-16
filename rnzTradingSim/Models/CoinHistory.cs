using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace rnzTradingSim.Models
{
  public class CoinHistory
  {
    [Key]
    public int Id { get; set; }
    public string CoinId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime Timestamp { get; set; }

    [ForeignKey("CoinId")]
    public Coin? Coin { get; set; }
  }
}