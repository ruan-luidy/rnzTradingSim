using Microsoft.EntityFrameworkCore;
using rnzTradingSim.Models;
using System.IO;

namespace rnzTradingSim.Data
{
  public class TradingDbContext : DbContext
  {
    public DbSet<Player> Players { get; set; }
    public DbSet<GameResult> GameResults { get; set; }
    public DbSet<Coin> Coins { get; set; }
    public DbSet<CoinHistory> CoinHistories { get; set; }

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

      // Habilitar logs para debug (opcional)
      optionsBuilder.EnableDetailedErrors();
      optionsBuilder.EnableSensitiveDataLogging();
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
        entity.Property(e => e.Image).HasMaxLength(500);

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

      base.OnModelCreating(modelBuilder);
    }

    // Método para inicializar o banco com verificações
    public void InitializeDatabase()
    {
      try
      {
        System.Diagnostics.Debug.WriteLine("Initializing database...");

        // Garantir que o banco e todas as tabelas sejam criadas
        var created = Database.EnsureCreated();

        if (created)
        {
          System.Diagnostics.Debug.WriteLine("Database created successfully");
        }
        else
        {
          System.Diagnostics.Debug.WriteLine("Database already exists");
        }

        // Verificar se as tabelas existem
        var tables = new[] { "Players", "GameResults", "Coins", "CoinHistories" };

        foreach (var table in tables)
        {
          var exists = Database.ExecuteSqlRaw($"SELECT name FROM sqlite_master WHERE type='table' AND name='{table}';") >= 0;
          System.Diagnostics.Debug.WriteLine($"Table {table}: {(exists ? "EXISTS" : "MISSING")}");
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error initializing database: {ex.Message}");
        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        throw;
      }
    }

    // Método para forçar migração se necessário
    public void EnsureMigrated()
    {
      try
      {
        Database.Migrate();
        System.Diagnostics.Debug.WriteLine("Database migrated successfully");
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error during migration: {ex.Message}");
        // Se a migração falhar, tenta criar o banco novamente
        try
        {
          Database.EnsureDeleted();
          Database.EnsureCreated();
          System.Diagnostics.Debug.WriteLine("Database recreated successfully");
        }
        catch (Exception recreateEx)
        {
          System.Diagnostics.Debug.WriteLine($"Error recreating database: {recreateEx.Message}");
          throw;
        }
      }
    }
  }
}