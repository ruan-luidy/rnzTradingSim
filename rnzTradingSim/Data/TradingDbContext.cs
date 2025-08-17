using Microsoft.EntityFrameworkCore;
using rnzTradingSim.Models;
using rnzTradingSim.Services;
using System.IO;
using static DatabaseConstants;

namespace rnzTradingSim.Data
{
  public class TradingDbContext : DbContext
  {
    public DbSet<Player> Players { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GameResult> GameResults { get; set; }
    public DbSet<Coin> Coins { get; set; }
    public DbSet<CoinHistory> CoinHistories { get; set; }
    
    // Novas entidades para trading estilo rugplay
    public DbSet<UserCoin> UserCoins { get; set; }
    public DbSet<Trade> Trades { get; set; }
    public DbSet<Portfolio> Portfolios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      var dbPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RnzTradingSim",
        "trading.db"
        );

      // Criar diretorio se nao existir
      var directory = Path.GetDirectoryName(dbPath);
      if (!Directory.Exists(directory))
      {
        Directory.CreateDirectory(directory);
      }

      optionsBuilder.UseSqlite($"Data Source={dbPath}");

      // Habilitar logs apenas em debug
#if DEBUG
      optionsBuilder.EnableDetailedErrors();
      optionsBuilder.EnableSensitiveDataLogging();
#endif
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      // Configuração da entidade Player
      modelBuilder.Entity<Player>(entity =>
      {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

        // Configurar todas as propriedades decimais com precisão 15,2
        entity.Property(e => e.Balance)
              .HasColumnType("decimal(15,2)")
              .HasPrecision(15, 2);

        entity.Property(e => e.TotalWagered)
              .HasColumnType("decimal(15,2)")
              .HasPrecision(15, 2);

        entity.Property(e => e.TotalWon)
              .HasColumnType("decimal(15,2)")
              .HasPrecision(15, 2);

        entity.Property(e => e.TotalLost)
              .HasColumnType("decimal(15,2)")
              .HasPrecision(15, 2);

        entity.Property(e => e.BiggestWin)
              .HasColumnType("decimal(15,2)")
              .HasPrecision(15, 2);

        entity.Property(e => e.BiggestLoss)
              .HasColumnType("decimal(15,2)")
              .HasPrecision(15, 2);
      });

      // Configuração da entidade Game
      modelBuilder.Entity<Game>(entity =>
      {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Type).HasConversion<string>().IsRequired();
        entity.Property(e => e.Status).HasConversion<string>().IsRequired();
        entity.Property(e => e.GameData).HasMaxLength(2000);

        entity.Property(e => e.BetAmount)
              .HasColumnType("decimal(15,2)")
              .HasPrecision(15, 2);

        entity.Property(e => e.CurrentPayout)
              .HasColumnType("decimal(15,2)")
              .HasPrecision(15, 2);

        entity.Property(e => e.Multiplier)
              .HasColumnType("decimal(8,4)")
              .HasPrecision(8, 4);

        // Relacionamento com Player
        entity.HasOne(e => e.Player)
              .WithMany()
              .HasForeignKey(e => e.PlayerId)
              .OnDelete(DeleteBehavior.Cascade);

        // Índices para performance
        entity.HasIndex(e => new { e.PlayerId, e.Status });
        entity.HasIndex(e => e.StartedAt);
      });

      // Configuração da entidade GameResult
      modelBuilder.Entity<GameResult>(entity =>
      {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.GameType).IsRequired().HasMaxLength(50);
        entity.Property(e => e.Details).HasMaxLength(1000);

        // Configurar propriedades decimais com precisão 15,2
        entity.Property(e => e.BetAmount)
              .HasColumnType("decimal(15,2)")
              .HasPrecision(15, 2);

        entity.Property(e => e.WinAmount)
              .HasColumnType("decimal(15,2)")
              .HasPrecision(15, 2);

        entity.Property(e => e.Multiplier)
              .HasColumnType("decimal(15,2)")
              .HasPrecision(15, 2);

        // Relacionamento com o Player
        entity.HasOne<Player>()
              .WithMany()
              .HasForeignKey(e => e.PlayerId)
              .OnDelete(DeleteBehavior.Cascade);
      });

      // Configuração da entidade Coin
      modelBuilder.Entity<Coin>(entity =>
      {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        entity.Property(e => e.Symbol).IsRequired().HasMaxLength(20);

        // Configurar preço com mais precisão para cryptos
        entity.Property(e => e.CurrentPrice)
              .HasColumnType("decimal(18,8)")
              .HasPrecision(18, 8);

        entity.Property(e => e.MarketCapValue)
              .HasColumnType("decimal(20,2)")
              .HasPrecision(20, 2);

        entity.Property(e => e.Volume24h)
              .HasColumnType("decimal(20,2)")
              .HasPrecision(20, 2);

        entity.Property(e => e.PriceChange24h)
              .HasColumnType("decimal(18,8)")
              .HasPrecision(18, 8);

        entity.Property(e => e.PriceChangePercentage24h)
              .HasColumnType("decimal(8,4)")
              .HasPrecision(8, 4);

        entity.Property(e => e.MarketCapRank).IsRequired();
        entity.Property(e => e.LastUpdated).IsRequired();

        // Índices para melhor performance
        entity.HasIndex(e => e.Symbol);
        entity.HasIndex(e => e.MarketCapRank);
        entity.HasIndex(e => e.LastUpdated);
      });

      // Configuração da entidade CoinHistory
      modelBuilder.Entity<CoinHistory>(entity =>
      {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.CoinId).IsRequired().HasMaxLength(100);

        entity.Property(e => e.Price)
              .HasColumnType("decimal(18,8)")
              .HasPrecision(18, 8);

        entity.Property(e => e.Timestamp).IsRequired();

        // Relacionamento com Coin
        entity.HasOne(e => e.Coin)
              .WithMany()
              .HasForeignKey(e => e.CoinId)
              .OnDelete(DeleteBehavior.Cascade);

        // Índices para consultas de histórico
        entity.HasIndex(e => new { e.CoinId, e.Timestamp });
        entity.HasIndex(e => e.Timestamp);
      });

      // Configuração da entidade UserCoin
      modelBuilder.Entity<UserCoin>(entity =>
      {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Symbol).IsRequired().HasMaxLength(10);
        entity.Property(e => e.Description).HasMaxLength(500);
        
        // Configurar decimais
        entity.Property(e => e.TotalSupply).HasPrecision(28, 8);
        entity.Property(e => e.CirculatingSupply).HasPrecision(28, 8);
        entity.Property(e => e.InitialPrice).HasPrecision(18, 8);
        entity.Property(e => e.CurrentPrice).HasPrecision(18, 8);
        entity.Property(e => e.PoolTokenAmount).HasPrecision(28, 8);
        entity.Property(e => e.PoolBaseAmount).HasPrecision(18, 8);
        entity.Property(e => e.Volume24h).HasPrecision(18, 8);
        entity.Property(e => e.PriceChange24h).HasPrecision(18, 8);
        entity.Property(e => e.AllTimeHigh).HasPrecision(18, 8);
        entity.Property(e => e.AllTimeLow).HasPrecision(18, 8);
        
        // Índices
        entity.HasIndex(e => e.Symbol).IsUnique();
        entity.HasIndex(e => e.CreatorId);
        entity.HasIndex(e => e.IsRugged);
        entity.HasIndex(e => e.CreatedAt);
      });
      
      // Configuração da entidade Trade
      modelBuilder.Entity<Trade>(entity =>
      {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Type).HasConversion<string>().IsRequired();
        
        // Configurar decimais
        entity.Property(e => e.TokenAmount).HasPrecision(28, 8);
        entity.Property(e => e.UsdAmount).HasPrecision(18, 8);
        entity.Property(e => e.PricePerToken).HasPrecision(18, 8);
        entity.Property(e => e.PriceImpact).HasPrecision(8, 4);
        entity.Property(e => e.Slippage).HasPrecision(8, 4);
        entity.Property(e => e.TradingFee).HasPrecision(18, 8);
        entity.Property(e => e.GasFee).HasPrecision(18, 8);
        entity.Property(e => e.PoolTokenAfter).HasPrecision(28, 8);
        entity.Property(e => e.PoolBaseAfter).HasPrecision(18, 8);
        entity.Property(e => e.PriceAfter).HasPrecision(18, 8);
        entity.Property(e => e.RealizedPnL).HasPrecision(18, 8);
        
        // Relacionamentos
        entity.HasOne(e => e.Coin)
              .WithMany()
              .HasForeignKey(e => e.CoinId)
              .OnDelete(DeleteBehavior.Restrict);
              
        entity.HasOne(e => e.Player)
              .WithMany()
              .HasForeignKey(e => e.PlayerId)
              .OnDelete(DeleteBehavior.Cascade);
        
        // Índices
        entity.HasIndex(e => new { e.CoinId, e.Timestamp });
        entity.HasIndex(e => new { e.PlayerId, e.Timestamp });
        entity.HasIndex(e => e.Type);
      });
      
      // Configuração da entidade Portfolio
      modelBuilder.Entity<Portfolio>(entity =>
      {
        entity.HasKey(e => e.Id);
        
        // Único portfolio por player/coin
        entity.HasIndex(e => new { e.PlayerId, e.CoinId }).IsUnique();
        
        // Configurar decimais
        entity.Property(e => e.TokenBalance).HasPrecision(28, 8);
        entity.Property(e => e.AverageBuyPrice).HasPrecision(18, 8);
        entity.Property(e => e.TotalInvested).HasPrecision(18, 8);
        entity.Property(e => e.RealizedPnL).HasPrecision(18, 8);
        entity.Property(e => e.LiquidityTokens).HasPrecision(28, 8);
        
        // Relacionamentos
        entity.HasOne(e => e.Player)
              .WithMany()
              .HasForeignKey(e => e.PlayerId)
              .OnDelete(DeleteBehavior.Cascade);
              
        entity.HasOne(e => e.Coin)
              .WithMany()
              .HasForeignKey(e => e.CoinId)
              .OnDelete(DeleteBehavior.Restrict);
      });

      base.OnModelCreating(modelBuilder);
    }

    // Método para inicializar o banco com verificações
    public void InitializeDatabase()
    {
      try
      {
        LoggingService.Info("Initializing database...");

        // Garantir que o banco e todas as tabelas sejam criadas
        var created = Database.EnsureCreated();

        if (created)
        {
          LoggingService.Info("Database created successfully");
        }
        else
        {
          LoggingService.Debug("Database already exists");
        }

        // Verificar se as tabelas existem usando Entity Framework
        var tables = new[] { 
          (nameof(Players), Players.Any()),
          (nameof(Games), Games.Any()),
          (nameof(GameResults), GameResults.Any()),
          (nameof(Coins), Coins.Any()),
          (nameof(CoinHistories), CoinHistories.Any())
        };

        foreach (var (tableName, hasData) in tables)
        {
          try
          {
            // Tentar acessar a tabela para verificar se existe
            var count = tableName switch
            {
              nameof(Players) => Players.Count(),
              nameof(Games) => Games.Count(),
              nameof(GameResults) => GameResults.Count(),
              nameof(Coins) => Coins.Count(),
              nameof(CoinHistories) => CoinHistories.Count(),
              _ => 0
            };
            LoggingService.Debug($"Table {tableName}: EXISTS ({count} records)");
          }
          catch
          {
            LoggingService.Warning($"Table {tableName}: MISSING or INACCESSIBLE");
          }
        }
      }
      catch (Exception ex)
      {
        LoggingService.Error("Error initializing database", ex);
        throw;
      }
    }

    // Método para forçar migração se necessário
    public void EnsureMigrated()
    {
      try
      {
        Database.Migrate();
        LoggingService.Info("Database migrated successfully");
      }
      catch (Exception ex)
      {
        LoggingService.Warning("Error during migration, attempting database recreation");
        // Se a migração falhar, tenta criar o banco novamente
        try
        {
          Database.EnsureDeleted();
          Database.EnsureCreated();
          LoggingService.Info("Database recreated successfully");
        }
        catch (Exception recreateEx)
        {
          LoggingService.Error("Error recreating database", recreateEx);
          throw;
        }
      }
    }

    // Método para limpeza automática de dados antigos
    public async Task CleanupOldDataAsync()
    {
      try
      {
        var cutoffDate = DateTime.Now.AddDays(-GAME_HISTORY_RETENTION_DAYS);
        
        // Limpar jogos antigos e finalizados
        var oldGames = await Games
          .Where(g => g.FinishedAt < cutoffDate && g.Status != GameStatus.InProgress)
          .ToListAsync();

        if (oldGames.Any())
        {
          Games.RemoveRange(oldGames);
          LoggingService.Info($"Cleaned up {oldGames.Count} old games");
        }

        // Limpar resultados de jogos antigos
        var oldResults = await GameResults
          .Where(gr => gr.PlayedAt < cutoffDate)
          .ToListAsync();

        if (oldResults.Any())
        {
          GameResults.RemoveRange(oldResults);
          LoggingService.Info($"Cleaned up {oldResults.Count} old game results");
        }

        // Limpar histórico de moedas antigo
        var oldCoinHistory = await CoinHistories
          .Where(ch => ch.Timestamp < cutoffDate)
          .ToListAsync();

        if (oldCoinHistory.Any())
        {
          CoinHistories.RemoveRange(oldCoinHistory);
          LoggingService.Info($"Cleaned up {oldCoinHistory.Count} old coin history records");
        }

        await SaveChangesAsync();
      }
      catch (Exception ex)
      {
        LoggingService.Error("Error during database cleanup", ex);
      }
    }
  }
}