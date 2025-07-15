using Microsoft.EntityFrameworkCore;
using rnzTradingSim.Models;
using System.IO;

namespace rnzTradingSim.Data
{
  public class TradingDbContext : DbContext
  {
    public DbSet<Player> Players { get; set; }
    public DbSet<GameResult> GameResults { get; set; }

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
        entity.Property(e => e.Balance).HasColumnType("decimal(15,2)");
        entity.Property(e => e.TotalWagered).HasColumnType("decimal(15,2)");
        entity.Property(e => e.TotalWon).HasColumnType("decimal(15,2)");
        entity.Property(e => e.TotalLost).HasColumnType("decimal(15,2)");
        entity.Property(e => e.BiggestWin).HasColumnType("decimal(15,2)");
        entity.Property(e => e.BiggestLoss).HasColumnType("decimal(15,2)");
      });

      // Configuraçao da entidade GameResult
      modelBuilder.Entity<GameResult>(entity =>
      {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.GameType).IsRequired().HasMaxLength(50);
        entity.Property(e => e.BetAmount).HasColumnType("decimal(15,2)");
        entity.Property(e => e.WinAmount).HasColumnType("decimal(15,2)");
        entity.Property(e => e.Multiplier).HasColumnType("decimal(15,2)");
        entity.Property(e => e.Details).HasMaxLength(1000);

        // Relacioanmento com o Player
        entity.HasOne<Player>()
              .WithMany()
              .HasForeignKey(e => e.PlayerId)
              .OnDelete(DeleteBehavior.Cascade);
      });

      base.OnModelCreating(modelBuilder);
    }
  }
}
