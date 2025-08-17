using Microsoft.EntityFrameworkCore;
using rnzTradingSim.Data;
using rnzTradingSim.Models;

namespace rnzTradingSim.Services
{
  public class CoinCreationService : IDisposable
  {
    private readonly TradingDbContext _context;
    private readonly PlayerService _playerService;
    private readonly Random _random = new();
    private bool _disposed = false;
    
    // Custos e limites
    public const decimal COIN_CREATION_COST = 10m; // $10 para criar uma moeda
    public const decimal MIN_INITIAL_LIQUIDITY = 100m; // Mínimo $100 de liquidez inicial
    public const decimal MAX_INITIAL_SUPPLY = 1_000_000_000m; // 1 bilhão max
    public const int MAX_COINS_PER_PLAYER = 10; // Máximo de moedas por jogador
    
    public CoinCreationService(TradingDbContext context, PlayerService playerService)
    {
      _context = context;
      _playerService = playerService;
    }
    
    public async Task<(bool success, string message, UserCoin? coin)> CreateCoinAsync(
      string name, 
      string symbol, 
      string description,
      decimal totalSupply,
      decimal initialLiquidity)
    {
      try
      {
        var player = _playerService.GetCurrentPlayer();
        
        // Validações
        if (player.Balance < COIN_CREATION_COST + initialLiquidity)
          return (false, $"Insufficient balance. Need ${COIN_CREATION_COST + initialLiquidity:N2}", null);
        
        if (initialLiquidity < MIN_INITIAL_LIQUIDITY)
          return (false, $"Minimum initial liquidity is ${MIN_INITIAL_LIQUIDITY:N2}", null);
        
        if (totalSupply > MAX_INITIAL_SUPPLY)
          return (false, $"Maximum supply is {MAX_INITIAL_SUPPLY:N0}", null);
        
        // Verificar limite de moedas
        var playerCoinsCount = await _context.UserCoins
          .CountAsync(c => c.CreatorId == player.Id.ToString());
        
        if (playerCoinsCount >= MAX_COINS_PER_PLAYER)
          return (false, $"Maximum {MAX_COINS_PER_PLAYER} coins per player", null);
        
        // Verificar se símbolo já existe
        var symbolExists = await _context.UserCoins
          .AnyAsync(c => c.Symbol.ToUpper() == symbol.ToUpper());
        
        if (symbolExists)
          return (false, "Symbol already exists", null);
        
        // Calcular preço inicial baseado na liquidez
        var creatorShare = totalSupply * 0.1m; // Criador fica com 10%
        var poolSupply = totalSupply * 0.9m; // 90% vai pro pool
        var initialPrice = initialLiquidity / poolSupply;
        
        // Criar a moeda
        var coin = new UserCoin
        {
          CreatorId = player.Id.ToString(),
          Name = name,
          Symbol = symbol.ToUpper(),
          Description = description,
          TotalSupply = totalSupply,
          CirculatingSupply = creatorShare, // Inicialmente só o que o criador tem
          InitialPrice = initialPrice,
          CurrentPrice = initialPrice,
          PoolTokenAmount = poolSupply,
          PoolBaseAmount = initialLiquidity,
          ImageUrl = GenerateRandomCoinImage(symbol),
          AllTimeHigh = initialPrice,
          AllTimeLow = initialPrice,
          TotalHolders = 1 // O criador
        };
        
        // Adicionar ao banco
        _context.UserCoins.Add(coin);
        
        // Criar portfolio do criador
        var portfolio = new Portfolio
        {
          PlayerId = player.Id,
          CoinId = coin.Id,
          TokenBalance = creatorShare,
          AverageBuyPrice = 0, // Criador recebe de graça
          TotalInvested = 0,
          FirstPurchase = DateTime.Now,
          LastTransaction = DateTime.Now
        };
        
        _context.Portfolios.Add(portfolio);
        
        // Registrar transação de mint
        var mintTrade = new Trade
        {
          CoinId = coin.Id,
          PlayerId = player.Id,
          Type = TradeType.Mint,
          TokenAmount = creatorShare,
          UsdAmount = 0,
          PricePerToken = 0,
          PoolTokenAfter = coin.PoolTokenAmount,
          PoolBaseAfter = coin.PoolBaseAmount,
          PriceAfter = coin.CurrentPrice
        };
        
        _context.Trades.Add(mintTrade);
        
        // Deduzir custo do player
        player.Balance -= (COIN_CREATION_COST + initialLiquidity);
        _playerService.SavePlayer(player);
        
        await _context.SaveChangesAsync();
        
        LoggingService.Info($"Coin {symbol} created by player {player.Id} with ${initialLiquidity:N2} liquidity");
        
        return (true, $"Coin {symbol} created successfully!", coin);
      }
      catch (Exception ex)
      {
        LoggingService.Error("Error creating coin", ex);
        return (false, "Error creating coin", null);
      }
    }
    
    private string GenerateRandomCoinImage(string symbol)
    {
      // Gerar uma imagem placeholder baseada no símbolo
      var colors = new[] { "FF6B6B", "4ECDC4", "45B7D1", "96CEB4", "FFEAA7", "DDA0DD", "98D8C8", "F7DC6F" };
      var color = colors[Math.Abs(symbol.GetHashCode()) % colors.Length];
      return $"https://via.placeholder.com/64/{color}/FFFFFF?text={symbol.Substring(0, Math.Min(3, symbol.Length))}";
    }
    
    public async Task<List<UserCoin>> GetTopCoinsAsync(int count = 50)
    {
      return await _context.UserCoins
        .Where(c => !c.IsRugged)
        .OrderByDescending(c => c.MarketCap)
        .Take(count)
        .AsNoTracking()
        .ToListAsync();
    }
    
    public async Task<List<UserCoin>> GetPlayerCoinsAsync(int playerId)
    {
      return await _context.UserCoins
        .Where(c => c.CreatorId == playerId.ToString())
        .OrderByDescending(c => c.CreatedAt)
        .AsNoTracking()
        .ToListAsync();
    }
    
    public async Task<UserCoin?> GetCoinAsync(string coinId)
    {
      return await _context.UserCoins
        .Include(c => c.Creator)
        .FirstOrDefaultAsync(c => c.Id == coinId);
    }
    
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
      if (!_disposed && disposing)
      {
        _disposed = true;
      }
    }
  }
}