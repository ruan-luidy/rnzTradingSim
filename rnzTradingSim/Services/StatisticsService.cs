using Microsoft.EntityFrameworkCore;
using rnzTradingSim.Data;
using rnzTradingSim.Models;

namespace rnzTradingSim.Services;

public class StatisticsService
{
  private readonly TradingDbContext _context;

  public StatisticsService()
  {
    _context = new TradingDbContext();
  }

  public PlayerStatistics GetPlayerStatistics(int playerId)
  {
    try
    {
      var player = _context.Players.Find(playerId);
      if (player == null) return new PlayerStatistics();

      var gameResults = _context.GameResults
          .Where(gr => gr.PlayerId == playerId)
          .ToList();

      var today = DateTime.Today;
      var thisWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
      var thisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

      var todayResults = gameResults.Where(gr => gr.PlayedAt.Date == today).ToList();
      var weekResults = gameResults.Where(gr => gr.PlayedAt.Date >= thisWeek).ToList();
      var monthResults = gameResults.Where(gr => gr.PlayedAt.Date >= thisMonth).ToList();

      return new PlayerStatistics
      {
        // Estatísticas gerais
        TotalGamesPlayed = player.GamesPlayed,
        TotalGamesWon = player.GamesWon,
        WinRate = player.WinRate,
        TotalWagered = player.TotalWagered,
        TotalWon = player.TotalWon,
        TotalLost = player.TotalLost,
        NetProfit = player.NetProfit,
        BiggestWin = player.BiggestWin,
        BiggestLoss = player.BiggestLoss,
        CurrentBalance = player.Balance,

        // Estatísticas diárias
        TodayGamesPlayed = todayResults.Count,
        TodayGamesWon = todayResults.Count(r => r.IsWin),
        TodayProfit = todayResults.Sum(r => r.NetResult),
        TodayWagered = todayResults.Sum(r => r.BetAmount),

        // Estatísticas semanais
        WeekGamesPlayed = weekResults.Count,
        WeekGamesWon = weekResults.Count(r => r.IsWin),
        WeekProfit = weekResults.Sum(r => r.NetResult),
        WeekWagered = weekResults.Sum(r => r.BetAmount),

        // Estatísticas mensais
        MonthGamesPlayed = monthResults.Count,
        MonthGamesWon = monthResults.Count(r => r.IsWin),
        MonthProfit = monthResults.Sum(r => r.NetResult),
        MonthWagered = monthResults.Sum(r => r.BetAmount),

        // Estatísticas por jogo
        GameStatistics = GetGameTypeStatistics(playerId, gameResults)
      };
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Error getting player statistics: {ex.Message}");
      return new PlayerStatistics();
    }
  }

  private List<GameTypeStatistics> GetGameTypeStatistics(int playerId, List<GameResult> gameResults)
  {
    return gameResults
        .GroupBy(gr => gr.GameType)
        .Select(g => new GameTypeStatistics
        {
          GameType = g.Key,
          GamesPlayed = g.Count(),
          GamesWon = g.Count(r => r.IsWin),
          WinRate = g.Count() > 0 ? (decimal)g.Count(r => r.IsWin) / g.Count() * 100 : 0,
          TotalWagered = g.Sum(r => r.BetAmount),
          TotalWon = g.Where(r => r.IsWin).Sum(r => r.WinAmount),
          TotalLost = g.Where(r => !r.IsWin).Sum(r => r.BetAmount),
          NetProfit = g.Sum(r => r.NetResult),
          BiggestWin = g.Where(r => r.IsWin).DefaultIfEmpty().Max(r => r?.NetResult ?? 0),
          BiggestLoss = g.Where(r => !r.IsWin).DefaultIfEmpty().Max(r => Math.Abs(r?.NetResult ?? 0)),
          AverageMultiplier = g.Where(r => r.IsWin && r.Multiplier > 0).DefaultIfEmpty().Average(r => r?.Multiplier ?? 0)
        })
        .OrderByDescending(g => g.GamesPlayed)
        .ToList();
  }

  public List<GameResult> GetRecentWins(int playerId, int count = 10)
  {
    try
    {
      return _context.GameResults
          .Where(gr => gr.PlayerId == playerId && gr.IsWin)
          .OrderByDescending(gr => gr.NetResult)
          .Take(count)
          .ToList();
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Error getting recent wins: {ex.Message}");
      return new List<GameResult>();
    }
  }

  public List<GameResult> GetRecentLosses(int playerId, int count = 10)
  {
    try
    {
      return _context.GameResults
          .Where(gr => gr.PlayerId == playerId && !gr.IsWin)
          .OrderByDescending(gr => Math.Abs(gr.NetResult))
          .Take(count)
          .ToList();
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Error getting recent losses: {ex.Message}");
      return new List<GameResult>();
    }
  }

  public decimal GetBalanceHistory(int playerId, DateTime fromDate)
  {
    try
    {
      var results = _context.GameResults
          .Where(gr => gr.PlayerId == playerId && gr.PlayedAt >= fromDate)
          .OrderBy(gr => gr.PlayedAt)
          .ToList();

      var player = _context.Players.Find(playerId);
      if (player == null) return 0;

      // Calcular saldo inicial subtraindo todos os resultados dos jogos
      var totalNetResult = results.Sum(r => r.NetResult);
      return player.Balance - totalNetResult;
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Error getting balance history: {ex.Message}");
      return 0;
    }
  }

  public void Dispose()
  {
    _context?.Dispose();
  }
}

public class PlayerStatistics
{
  // Estatísticas gerais
  public int TotalGamesPlayed { get; set; }
  public int TotalGamesWon { get; set; }
  public decimal WinRate { get; set; }
  public decimal TotalWagered { get; set; }
  public decimal TotalWon { get; set; }
  public decimal TotalLost { get; set; }
  public decimal NetProfit { get; set; }
  public decimal BiggestWin { get; set; }
  public decimal BiggestLoss { get; set; }
  public decimal CurrentBalance { get; set; }

  // Estatísticas diárias
  public int TodayGamesPlayed { get; set; }
  public int TodayGamesWon { get; set; }
  public decimal TodayProfit { get; set; }
  public decimal TodayWagered { get; set; }
  public decimal TodayWinRate => TodayGamesPlayed > 0 ? (decimal)TodayGamesWon / TodayGamesPlayed * 100 : 0;

  // Estatísticas semanais
  public int WeekGamesPlayed { get; set; }
  public int WeekGamesWon { get; set; }
  public decimal WeekProfit { get; set; }
  public decimal WeekWagered { get; set; }
  public decimal WeekWinRate => WeekGamesPlayed > 0 ? (decimal)WeekGamesWon / WeekGamesPlayed * 100 : 0;

  // Estatísticas mensais
  public int MonthGamesPlayed { get; set; }
  public int MonthGamesWon { get; set; }
  public decimal MonthProfit { get; set; }
  public decimal MonthWagered { get; set; }
  public decimal MonthWinRate => MonthGamesPlayed > 0 ? (decimal)MonthGamesWon / MonthGamesPlayed * 100 : 0;

  // Estatísticas por tipo de jogo
  public List<GameTypeStatistics> GameStatistics { get; set; } = new List<GameTypeStatistics>();
}

public class GameTypeStatistics
{
  public string GameType { get; set; } = string.Empty;
  public int GamesPlayed { get; set; }
  public int GamesWon { get; set; }
  public decimal WinRate { get; set; }
  public decimal TotalWagered { get; set; }
  public decimal TotalWon { get; set; }
  public decimal TotalLost { get; set; }
  public decimal NetProfit { get; set; }
  public decimal BiggestWin { get; set; }
  public decimal BiggestLoss { get; set; }
  public decimal AverageMultiplier { get; set; }
}