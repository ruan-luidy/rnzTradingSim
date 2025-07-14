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
        return player ?? CreateNewPlayer();
      }
      catch
      {
        return CreateNewPlayer();
      }
    }

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

    SavePlayer(player);
    AddGameResult(result);
  }
}