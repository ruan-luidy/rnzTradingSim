using Microsoft.EntityFrameworkCore;
using rnzTradingSim.Data;
using rnzTradingSim.Models;
using static GameConstants;

namespace rnzTradingSim.Services;

public class PlayerService
{
  private readonly TradingDbContext _context;
  private Player _currentPlayer;

  public PlayerService()
  {
    _context = new TradingDbContext();
    InitializeDatabase();
    _currentPlayer = LoadOrCreatePlayer();
  }

  private void InitializeDatabase()
  {
    try
    {
      _context.Database.EnsureCreated();
      System.Diagnostics.Debug.WriteLine("Database initialized successfully");
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Error initializing database: {ex.Message}");
    }
  }

  public Player GetCurrentPlayer()
  {
    if (_currentPlayer == null)
    {
      _currentPlayer = LoadOrCreatePlayer();
    }

    // Verificação adicional para garantir que o player tem saldo válido
    if (_currentPlayer.Balance <= 0)
    {
      _currentPlayer.Balance = DEFAULT_BALANCE;
      SavePlayer(_currentPlayer);
    }

    System.Diagnostics.Debug.WriteLine($"Current player balance: {_currentPlayer.Balance}");
    return _currentPlayer;
  }

  public void SavePlayer(Player player)
  {
    try
    {
      _context.Players.Update(player);
      _context.SaveChanges();
      System.Diagnostics.Debug.WriteLine($"Player saved with balance: {player.Balance}");
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Error saving player: {ex.Message}");
    }
  }

  public void AddGameResult(GameResult result)
  {
    try
    {
      result.PlayerId = _currentPlayer.Id;
      _context.GameResults.Add(result);
      _context.SaveChanges();

      // Manter apenas os últimos 1000 jogos por player
      var oldResults = _context.GameResults
          .Where(gr => gr.PlayerId == _currentPlayer.Id)
          .OrderBy(gr => gr.PlayedAt)
          .Take(_context.GameResults.Count(gr => gr.PlayerId == _currentPlayer.Id) - 1000)
          .ToList();

      if (oldResults.Any())
      {
        _context.GameResults.RemoveRange(oldResults);
        _context.SaveChanges();
      }
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Error adding game result: {ex.Message}");
    }
  }

  public List<GameResult> GetGameHistory(int count = 50)
  {
    try
    {
      return _context.GameResults
          .Where(gr => gr.PlayerId == _currentPlayer.Id)
          .OrderByDescending(gr => gr.PlayedAt)
          .Take(count)
          .ToList();
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Error getting game history: {ex.Message}");
      return new List<GameResult>();
    }
  }

  public List<GameResult> GetGameHistoryByType(string gameType, int count = 50)
  {
    try
    {
      return _context.GameResults
          .Where(gr => gr.PlayerId == _currentPlayer.Id && gr.GameType == gameType)
          .OrderByDescending(gr => gr.PlayedAt)
          .Take(count)
          .ToList();
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Error getting game history by type: {ex.Message}");
      return new List<GameResult>();
    }
  }

  private Player LoadOrCreatePlayer()
  {
    try
    {
      var player = _context.Players.FirstOrDefault();

      if (player != null)
      {
        System.Diagnostics.Debug.WriteLine($"Loaded player with balance: {player.Balance}");
        return player;
      }
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Error loading player: {ex.Message}");
    }

    System.Diagnostics.Debug.WriteLine("Creating new player");
    return CreateNewPlayer();
  }

  private Player CreateNewPlayer()
  {
    try
    {
      var player = new Player
      {
        Name = "Player",
        Balance = DEFAULT_BALANCE,
        CreatedAt = DateTime.Now
      };

      _context.Players.Add(player);
      _context.SaveChanges();

      System.Diagnostics.Debug.WriteLine($"New player created with balance: {player.Balance}");
      return player;
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Error creating new player: {ex.Message}");
      // Retornar player padrão se falhar
      return new Player
      {
        Name = "Player",
        Balance = DEFAULT_BALANCE,
        CreatedAt = DateTime.Now
      };
    }
  }

  public void UpdatePlayerStats(Player player, GameResult result)
  {
    try
    {
      player.TotalWagered += result.BetAmount;
      player.GamesPlayed++;

      if (result.IsWin)
      {
        player.GamesWon++;
        player.TotalWon += result.WinAmount;
        player.Balance += result.NetResult;

        if (result.NetResult > player.BiggestWin)
          player.BiggestWin = result.NetResult;
      }
      else
      {
        player.TotalLost += result.BetAmount;
        player.Balance += result.NetResult; // NetResult é negativo para perdas

        if (Math.Abs(result.NetResult) > player.BiggestLoss)
          player.BiggestLoss = Math.Abs(result.NetResult);
      }

      // Garantir que o saldo nunca seja negativo
      if (player.Balance < 0)
      {
        player.Balance = 0;
      }

      SavePlayer(player);
      AddGameResult(result);

      System.Diagnostics.Debug.WriteLine($"Player stats updated. New balance: {player.Balance}");
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Error updating player stats: {ex.Message}");
    }
  }

  // Método para resetar o player para debugging
  public void ResetPlayer()
  {
    try
    {
      // Remover histórico de jogos do player atual
      var gameResults = _context.GameResults
          .Where(gr => gr.PlayerId == _currentPlayer.Id)
          .ToList();

      if (gameResults.Any())
      {
        _context.GameResults.RemoveRange(gameResults);
      }

      // Resetar estatísticas do player
      _currentPlayer.Balance = DEFAULT_BALANCE;
      _currentPlayer.TotalWagered = 0m;
      _currentPlayer.TotalWon = 0m;
      _currentPlayer.TotalLost = 0m;
      _currentPlayer.GamesPlayed = 0;
      _currentPlayer.GamesWon = 0;
      _currentPlayer.BiggestWin = 0m;
      _currentPlayer.BiggestLoss = 0m;
      _currentPlayer.CreatedAt = DateTime.Now;

      SavePlayer(_currentPlayer);

      System.Diagnostics.Debug.WriteLine("Player data reset");
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Error resetting player: {ex.Message}");
    }
  }

  public void Dispose()
  {
    _context?.Dispose();
  }
}