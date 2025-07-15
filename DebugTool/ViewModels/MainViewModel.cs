using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using rnzTradingSim.Services;
using rnzTradingSim.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace DebugTool.ViewModels;

public partial class MainViewModel : ObservableObject
{
  private readonly PlayerService _playerService;

  [ObservableProperty]
  private Player currentPlayer;

  [ObservableProperty]
  private decimal newBalance = 10000m;

  [ObservableProperty]
  private string logText = "";

  [ObservableProperty]
  private bool isPlayerLoaded = false;

  public ObservableCollection<GameResult> GameHistory { get; }
  public ObservableCollection<string> LogEntries { get; }

  public MainViewModel()
  {
    _playerService = new PlayerService();
    GameHistory = new ObservableCollection<GameResult>();
    LogEntries = new ObservableCollection<string>();

    LoadPlayer();
    LoadGameHistory();
    AddLog("Debug Tool inicializado");
  }

  [RelayCommand]
  private void LoadPlayer()
  {
    try
    {
      CurrentPlayer = _playerService.GetCurrentPlayer();
      IsPlayerLoaded = true;
      NewBalance = CurrentPlayer.Balance;
      AddLog($"Player carregado - Saldo: {CurrentPlayer.Balance:C}");
    }
    catch (Exception ex)
    {
      AddLog($"Erro ao carregar player: {ex.Message}");
    }
  }

  [RelayCommand]
  private void SavePlayer()
  {
    if (CurrentPlayer == null) return;

    try
    {
      _playerService.SavePlayer(CurrentPlayer);
      AddLog($"Player salvo - Saldo: {CurrentPlayer.Balance:C}");
    }
    catch (Exception ex)
    {
      AddLog($"Erro ao salvar player: {ex.Message}");
    }
  }

  [RelayCommand]
  private void UpdateBalance()
  {
    if (CurrentPlayer == null) return;

    CurrentPlayer.Balance = NewBalance;
    SavePlayer();
    AddLog($"Saldo atualizado para: {NewBalance:C}");
  }

  [RelayCommand]
  private void ResetPlayer()
  {
    try
    {
      _playerService.ResetPlayer();
      LoadPlayer();
      LoadGameHistory();
      AddLog("Player resetado para valores padrão");
    }
    catch (Exception ex)
    {
      AddLog($"Erro ao resetar player: {ex.Message}");
    }
  }

  [RelayCommand]
  private void AddTestWin()
  {
    if (CurrentPlayer == null) return;

    var testResult = new GameResult
    {
      GameType = "Mines",
      BetAmount = 100m,
      WinAmount = 250m,
      IsWin = true,
      Multiplier = 2.5m,
      Details = "{\"mines\":3,\"revealed\":2}"
    };

    _playerService.UpdatePlayerStats(CurrentPlayer, testResult);
    LoadPlayer();
    LoadGameHistory();
    AddLog($"Vitória teste adicionada: +{testResult.NetResult:C}");
  }

  [RelayCommand]
  private void AddTestLoss()
  {
    if (CurrentPlayer == null) return;

    var testResult = new GameResult
    {
      GameType = "Mines",
      BetAmount = 50m,
      WinAmount = 0m,
      IsWin = false,
      Multiplier = 0m,
      Details = "{\"mines\":3,\"revealed\":1}"
    };

    _playerService.UpdatePlayerStats(CurrentPlayer, testResult);
    LoadPlayer();
    LoadGameHistory();
    AddLog($"Derrota teste adicionada: {testResult.NetResult:C}");
  }

  [RelayCommand]
  private void ClearHistory()
  {
    try
    {
      if (File.Exists("history.json"))
      {
        File.Delete("history.json");
      }
      LoadGameHistory();
      AddLog("Histórico de jogos limpo");
    }
    catch (Exception ex)
    {
      AddLog($"Erro ao limpar histórico: {ex.Message}");
    }
  }

  [RelayCommand]
  private void OpenDataFolder()
  {
    try
    {
      var currentDir = Directory.GetCurrentDirectory();
      Process.Start("explorer.exe", currentDir);
      AddLog($"Pasta de dados aberta: {currentDir}");
    }
    catch (Exception ex)
    {
      AddLog($"Erro ao abrir pasta: {ex.Message}");
    }
  }

  [RelayCommand]
  private void ExportData()
  {
    try
    {
      var exportData = new
      {
        Player = CurrentPlayer,
        History = _playerService.GetGameHistory(),
        ExportedAt = DateTime.Now
      };

      var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
      {
        WriteIndented = true
      });

      var fileName = $"export_{DateTime.Now:yyyyMMdd_HHmmss}.json";
      File.WriteAllText(fileName, json);
      AddLog($"Dados exportados para: {fileName}");
    }
    catch (Exception ex)
    {
      AddLog($"Erro ao exportar dados: {ex.Message}");
    }
  }

  [RelayCommand]
  private void ClearLog()
  {
    LogEntries.Clear();
    LogText = "";
  }

  private void LoadGameHistory()
  {
    GameHistory.Clear();
    var history = _playerService.GetGameHistory();

    foreach (var game in history.Take(20).OrderByDescending(g => g.PlayedAt))
    {
      GameHistory.Add(game);
    }

    AddLog($"Histórico carregado: {history.Count} jogos");
  }

  private void AddLog(string message)
  {
    var entry = $"[{DateTime.Now:HH:mm:ss}] {message}";
    LogEntries.Insert(0, entry);
    LogText = string.Join("\n", LogEntries);

    // Manter apenas as últimas 100 entradas
    while (LogEntries.Count > 100)
    {
      LogEntries.RemoveAt(LogEntries.Count - 1);
    }
  }

  // Métodos para modificar estatísticas específicas
  [RelayCommand]
  private void AddGamesPlayed(string amount)
  {
    if (CurrentPlayer == null || !int.TryParse(amount, out int games)) return;

    CurrentPlayer.GamesPlayed += games;
    SavePlayer();
    AddLog($"Adicionados {games} jogos played");
  }

  [RelayCommand]
  private void AddTotalWagered(string amount)
  {
    if (CurrentPlayer == null || !decimal.TryParse(amount, out decimal wagered)) return;

    CurrentPlayer.TotalWagered += wagered;
    SavePlayer();
    AddLog($"Adicionados {wagered:C} em total wagered");
  }

  [RelayCommand]
  private void SetBiggestWin(string amount)
  {
    if (CurrentPlayer == null || !decimal.TryParse(amount, out decimal win)) return;

    CurrentPlayer.BiggestWin = win;
    SavePlayer();
    AddLog($"Biggest win definido para {win:C}");
  }

  [RelayCommand]
  private void SetBiggestLoss(string amount)
  {
    if (CurrentPlayer == null || !decimal.TryParse(amount, out decimal loss)) return;

    CurrentPlayer.BiggestLoss = loss;
    SavePlayer();
    AddLog($"Biggest loss definido para {loss:C}");
  }
}