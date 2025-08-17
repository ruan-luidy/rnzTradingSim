using rnzTradingSim.Models;
using static GameConstants;

namespace rnzTradingSim.Helpers
{
  public static class GameLogic
  {
    private static readonly Random _random = new();

    public static class Coinflip
    {
      public static (bool isWin, decimal payout) Play(decimal betAmount, bool userChoice)
      {
        var result = _random.Next(2) == 0; // 0 = heads, 1 = tails
        var isWin = result == userChoice;
        var payout = isWin ? betAmount * 2m * COINFLIP_HOUSE_EDGE : 0m;
        return (isWin, payout);
      }
    }

    public static class Dice
    {
      public static (bool isWin, decimal payout, int rolledNumber) Play(decimal betAmount, int targetNumber, bool isOver)
      {
        var rolled = _random.Next(1, 101); // 1-100
        var isWin = isOver ? rolled > targetNumber : rolled < targetNumber;
        
        var winChance = isOver ? (100 - targetNumber) / 100m : (targetNumber - 1) / 100m;
        var multiplier = winChance > 0 ? (1m / winChance) * 0.98m : 0m; // 2% house edge
        
        var payout = isWin ? betAmount * multiplier : 0m;
        return (isWin, payout, rolled);
      }
    }

    public static class Mines
    {
      public class MineField
      {
        public List<bool> Field { get; set; } = new();
        public List<int> RevealedPositions { get; set; } = new();
        public int MineCount { get; set; }
        public decimal BetAmount { get; set; }
        public decimal CurrentMultiplier { get; set; } = 1m;
        private readonly Random _random = new();

        public void Initialize(int mineCount, decimal betAmount)
        {
          MineCount = mineCount;
          BetAmount = betAmount;
          Field = GenerateField(mineCount);
          RevealedPositions.Clear();
          CurrentMultiplier = 1m;
        }

        private List<bool> GenerateField(int mineCount)
        {
          var field = new List<bool>(new bool[25]); // 5x5 grid
          var minePositions = new HashSet<int>();
          
          while (minePositions.Count < mineCount)
          {
            minePositions.Add(_random.Next(25));
          }

          foreach (var pos in minePositions)
          {
            field[pos] = true; // true = mine
          }

          return field;
        }

        public (bool isMine, bool gameOver, decimal currentPayout) RevealTile(int position)
        {
          if (RevealedPositions.Contains(position))
            return (false, false, GetCurrentPayout());

          RevealedPositions.Add(position);
          var isMine = Field[position];

          if (isMine)
          {
            return (true, true, 0m);
          }

          UpdateMultiplier();
          return (false, false, GetCurrentPayout());
        }

        private void UpdateMultiplier()
        {
          var safeSpots = 25 - MineCount;
          var revealed = RevealedPositions.Count;
          var remaining = safeSpots - revealed;

          if (remaining > 0)
          {
            var factor = (decimal)safeSpots / remaining;
            CurrentMultiplier *= factor * MINES_HOUSE_EDGE;
          }
        }

        public decimal GetCurrentPayout()
        {
          return BetAmount * CurrentMultiplier;
        }
      }

      public static MineField CreateMineField(int mineCount, decimal betAmount)
      {
        var field = new MineField();
        field.Initialize(mineCount, betAmount);
        return field;
      }
    }

    public static class Slots
    {
      private static readonly string[] _symbols = { "🍒", "🍋", "🔔", "⭐", "💎" };
      
      public static (bool isWin, decimal payout, string[] result) Play(decimal betAmount)
      {
        var result = new string[3];
        for (int i = 0; i < 3; i++)
        {
          result[i] = _symbols[_random.Next(_symbols.Length)];
        }

        var multiplier = CalculateMultiplier(result);
        var isWin = multiplier > 0;
        var payout = isWin ? betAmount * multiplier : 0m;

        return (isWin, payout, result);
      }

      private static decimal CalculateMultiplier(string[] result)
      {
        // Three of a kind
        if (result[0] == result[1] && result[1] == result[2])
        {
          return result[0] switch
          {
            "💎" => 50m,
            "⭐" => 25m,
            "🔔" => 15m,
            "🍋" => 10m,
            "🍒" => 5m,
            _ => 0m
          };
        }

        // Two of a kind
        var pairs = result.GroupBy(x => x).Where(g => g.Count() == 2);
        if (pairs.Any())
        {
          var symbol = pairs.First().Key;
          return symbol switch
          {
            "💎" => 5m,
            "⭐" => 3m,
            "🔔" => 2m,
            _ => 0m
          };
        }

        return 0m;
      }
    }

    public static class Probability
    {
      public static decimal CalculateWinProbability(GameType gameType, string gameData)
      {
        return gameType switch
        {
          GameType.Coinflip => 0.5m,
          GameType.Dice => CalculateDiceProbability(gameData),
          GameType.Mines => CalculateMinesProbability(gameData),
          GameType.Slots => 0.15m, // Approximate
          _ => 0m
        };
      }

      private static decimal CalculateDiceProbability(string gameData)
      {
        // Parse gameData to get target number and direction
        // For now, return a default
        return 0.49m;
      }

      private static decimal CalculateMinesProbability(string gameData)
      {
        // Parse gameData to get mine count and revealed tiles
        // For now, return a default
        return 0.5m;
      }
    }

    public static bool ValidateBet(decimal betAmount, decimal playerBalance)
    {
      return betAmount >= MIN_BET && 
             betAmount <= MAX_BET && 
             betAmount <= playerBalance;
    }

    public static decimal CalculateMaxPayout(decimal betAmount, GameType gameType)
    {
      var maxMultiplier = gameType switch
      {
        GameType.Coinflip => 2m,
        GameType.Dice => 99m,
        GameType.Mines => 100m,
        GameType.Slots => 50m,
        _ => 1m
      };

      return Math.Min(betAmount * maxMultiplier, MAX_PAYOUT);
    }
  }
}
