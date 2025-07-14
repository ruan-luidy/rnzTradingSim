using System.IO;
using System.Text.Json;
using rnzTradingSim.Models;

namespace rnzTradingSim.Services;

public class PlayerService
{
  private const string PlayerFileName = "player.json";
  private const string HistoryFileName = "history.json";

  public Player GetCurrentPlayer()
  {
    if (File.Exists(PlayerFileName))
    {
      try
      {
        var json = File.ReadAllText(PlayerFileName);
        var player = JsonSerializer.Deserialize<Player>(json);

        // Verificação adicional para garantir que o player tem saldo válido
        if (player != null)
        {
          // Se o saldo for 0 ou negativo, resetar para o valor inicial
          if (player.Balance <= 0)
          {
            player.Balance = 10000m;
            SavePlayer(player);
          }

          System.Diagnostics.Debug.WriteLine($"Loaded player with balance: {player.Balance}");
          return player;
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error loading player: {ex.Message}");
      }
    }

    System.Diagnostics.Debug.WriteLine("Creating new player");
    return CreateNewPlayer();
  }

  public void SavePlayer(Player player)
  {
    try
    {
      var json = JsonSerializer.Serialize(player, new JsonSerializerOptions
      {
        WriteIndented = true
      });
      File.WriteAllText(PlayerFileName, json);
      System.Diagnostics.Debug.WriteLine($"Player saved with balance: {player.Balance}");
    }
    catch (Exception ex)
    {
      // Log error - for now just ignore
      System.Diagnostics.Debug.WriteLine($"Error saving player: {ex.Message}");
    }
  }

  public void AddGameResult(GameResult result)
  {
    var history = GetGameHistory();
    history.Add(result);

    // Keep only last 1000 games
    if (history.Count > 1000)
    {
      history = history.TakeLast(1000).ToList();
    }

    SaveGameHistory(history);
  }

  public List<GameResult> GetGameHistory()
  {
    if (File.Exists(HistoryFileName))
    {
      try
      {
        var json = File.ReadAllText(HistoryFileName);
        return JsonSerializer.Deserialize<List<GameResult>>(json) ?? new List<GameResult>();
      }
      catch
      {
        return new List<GameResult>();
      }
    }

    return new List<GameResult>();
  }

  private void SaveGameHistory(List<GameResult> history)
  {
    try
    {
      var json = JsonSerializer.Serialize(history, new JsonSerializerOptions
      {
        WriteIndented = true
      });
      File.WriteAllText(HistoryFileName, json);
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Error saving history: {ex.Message}");
    }
  }

  private Player CreateNewPlayer()
  {
    var player = new Player
    {
      Name = "Player",
      Balance = 10000m,
      CreatedAt = DateTime.Now
    };

    SavePlayer(player);
    System.Diagnostics.Debug.WriteLine($"New player created with balance: {player.Balance}");
    return player;
  }

  public void UpdatePlayerStats(Player player, GameResult result)
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
      player.Balance += result.NetResult; // NetResult is negative for losses

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

  // Método para resetar o player para debugging
  public void ResetPlayer()
  {
    if (File.Exists(PlayerFileName))
    {
      File.Delete(PlayerFileName);
    }
    if (File.Exists(HistoryFileName))
    {
      File.Delete(HistoryFileName);
    }
    System.Diagnostics.Debug.WriteLine("Player data reset");
  }
}