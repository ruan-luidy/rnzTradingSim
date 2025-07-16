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
        entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Symbol).IsRequired().HasMaxLength(10);
        entity.Property(e => e.Image).HasMaxLength(300);
        entity.Property(e => e.CurrentPrice).HasColumnType("decimal(15,6)").HasPrecision(15, 6);
        entity.Property(e => e.MarketCapValue).HasColumnType("decimal(20,2)").HasPrecision(20, 2);
        entity.Property(e => e.Volume24h).HasColumnType("decimal(20,2)").HasPrecision(20, 2);
        entity.Property(e => e.PriceChange24h).HasColumnType("decimal(15,6)").HasPrecision(15, 6);
        entity.Property(e => e.PriceChangePercentage24h).HasColumnType("decimal(8,2)").HasPrecision(8, 2);
        entity.Property(e => e.MarketCapRank);
        entity.Property(e => e.LastUpdated);
      });

      // Configuração da entidade CoinHistory
      modelBuilder.Entity<CoinHistory>(entity =>
      {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.CoinId).IsRequired();
        entity.Property(e => e.Price).HasColumnType("decimal(15,6)").HasPrecision(15, 6);
        entity.Property(e => e.Timestamp).IsRequired();
        entity.HasOne(e => e.Coin)
              .WithMany()
              .HasForeignKey(e => e.CoinId)
              .OnDelete(DeleteBehavior.Cascade);
      });

      base.OnModelCreating(modelBuilder);
    }
  }
}