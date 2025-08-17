using Microsoft.EntityFrameworkCore;
using rnzTradingSim.Data;
using rnzTradingSim.Models;
using static GameConstants;

namespace rnzTradingSim.Services
{
  public class GameService : IDisposable
  {
    private readonly TradingDbContext _context;
    private readonly PlayerService _playerService;

    public GameService(PlayerService playerService)
    {
      _context = new TradingDbContext();
      _playerService = playerService;
    }

    public async Task<Game> StartGameAsync(GameType gameType, decimal betAmount, string? gameData = null)
    {
      var player = _playerService.GetCurrentPlayer();
      
      if (betAmount < MIN_BET || betAmount > MAX_BET)
        throw new ArgumentException($"Bet amount must be between {MIN_BET} and {MAX_BET}");

      if (player.Balance < betAmount)
        throw new InvalidOperationException("Insufficient balance");

      var game = new Game
      {
        PlayerId = player.Id,
        Type = gameType,
        BetAmount = betAmount,
        GameData = gameData ?? "{}"
      };

      _context.Games.Add(game);
      await _context.SaveChangesAsync();

      return game;
    }

    public async Task<GameResult> FinishGameAsync(int gameId, GameStatus status, decimal payout = 0m, decimal multiplier = 1m)
    {
      var game = await _context.Games.FindAsync(gameId);
      if (game == null)
        throw new ArgumentException("Game not found");

      if (!game.IsActive)
        throw new InvalidOperationException("Game is not active");

      game.FinishGame(status, payout);
      game.Multiplier = multiplier;

      var gameResult = game.ToGameResult();
      var player = _playerService.GetCurrentPlayer();
      
      _playerService.UpdatePlayerStats(player, gameResult);

      _context.Games.Update(game);
      await _context.SaveChangesAsync();

      return gameResult;
    }

    public async Task<List<Game>> GetActiveGamesAsync(int playerId)
    {
      return await _context.Games
        .Where(g => g.PlayerId == playerId && g.Status == GameStatus.InProgress)
        .OrderByDescending(g => g.StartedAt)
        .ToListAsync();
    }

    public async Task<List<Game>> GetGameHistoryAsync(int playerId, int count = 50)
    {
      return await _context.Games
        .Where(g => g.PlayerId == playerId && g.Status != GameStatus.InProgress)
        .OrderByDescending(g => g.FinishedAt)
        .Take(count)
        .ToListAsync();
    }

    public async Task<Game?> GetGameAsync(int gameId)
    {
      return await _context.Games.FindAsync(gameId);
    }

    public async Task CancelGameAsync(int gameId)
    {
      var game = await _context.Games.FindAsync(gameId);
      if (game?.IsActive == true)
      {
        game.FinishGame(GameStatus.Cancelled);
        _context.Games.Update(game);
        await _context.SaveChangesAsync();
      }
    }

    public async Task CleanupOldGamesAsync(TimeSpan maxAge)
    {
      var cutoffDate = DateTime.Now.Subtract(maxAge);
      var oldGames = await _context.Games
        .Where(g => g.StartedAt < cutoffDate && g.Status != GameStatus.InProgress)
        .ToListAsync();

      if (oldGames.Any())
      {
        _context.Games.RemoveRange(oldGames);
        await _context.SaveChangesAsync();
      }
    }

    public void Dispose()
    {
      _context?.Dispose();
    }
  }
}
