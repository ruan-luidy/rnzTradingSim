using rnzTradingSim.Data;
using rnzTradingSim.Services;

namespace rnzTradingSim.Services
{
  public static class DatabaseInitializer
  {
    public static void Initialize()
    {
      try
      {
        System.Diagnostics.Debug.WriteLine("Starting database initialization...");

        // Inicializar o contexto principal
        using (var context = new TradingDbContext())
        {
          // Inicializar as tabelas principais
          context.InitializeDatabase();

          // Verificar se as tabelas foram criadas
          var playersExist = context.Players.Any();
          var coinsExist = context.Coins.Any();

          System.Diagnostics.Debug.WriteLine($"Players table populated: {playersExist}");
          System.Diagnostics.Debug.WriteLine($"Coins table populated: {coinsExist}");
        }

        // Inicializar o serviço de moedas fake
        using (var coinContext = new TradingDbContext())
        {
          var coinService = new FakeCoinService(coinContext);
          var coinCount = coinService.GetTotalCoinsCount();
          System.Diagnostics.Debug.WriteLine($"Total coins available: {coinCount}");

          // Se não há moedas, algo deu errado na inicialização
          if (coinCount == 0)
          {
            System.Diagnostics.Debug.WriteLine("No coins found, there might be an issue with coin initialization");
          }
        }

        // Inicializar o serviço de player
        var playerService = new PlayerService();
        var currentPlayer = playerService.GetCurrentPlayer();
        System.Diagnostics.Debug.WriteLine($"Current player initialized with balance: {currentPlayer.Balance}");
        playerService.Dispose();

        System.Diagnostics.Debug.WriteLine("Database initialization completed successfully!");
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error during database initialization: {ex.Message}");
        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

        // Tentar recriar o banco se algo deu errado
        try
        {
          System.Diagnostics.Debug.WriteLine("Attempting to recreate database...");

          using (var context = new TradingDbContext())
          {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            System.Diagnostics.Debug.WriteLine("Database recreated successfully");
          }

          // Tentar inicializar novamente
          using (var context = new TradingDbContext())
          {
            var coinService = new FakeCoinService(context);
            var coinCount = coinService.GetTotalCoinsCount();
            System.Diagnostics.Debug.WriteLine($"After recreation - Total coins: {coinCount}");
          }
        }
        catch (Exception recreateEx)
        {
          System.Diagnostics.Debug.WriteLine($"Failed to recreate database: {recreateEx.Message}");
          throw new InvalidOperationException("Failed to initialize database", recreateEx);
        }
      }
    }

    public static void ResetAllData()
    {
      try
      {
        System.Diagnostics.Debug.WriteLine("Resetting all database data...");

        using (var context = new TradingDbContext())
        {
          // Deletar e recriar o banco
          context.Database.EnsureDeleted();
          context.Database.EnsureCreated();

          System.Diagnostics.Debug.WriteLine("Database reset and recreated");
        }

        // Reinicializar tudo
        Initialize();

        System.Diagnostics.Debug.WriteLine("All data reset successfully!");
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error resetting database: {ex.Message}");
        throw;
      }
    }

    public static void VerifyDatabaseIntegrity()
    {
      try
      {
        using (var context = new TradingDbContext())
        {
          var tables = new Dictionary<string, Func<int>>
          {
            { "Players", () => context.Players.Count() },
            { "GameResults", () => context.GameResults.Count() },
            { "Coins", () => context.Coins.Count() },
            { "CoinHistories", () => context.CoinHistories.Count() }
          };

          System.Diagnostics.Debug.WriteLine("=== Database Integrity Check ===");

          foreach (var table in tables)
          {
            try
            {
              var count = table.Value();
              System.Diagnostics.Debug.WriteLine($"{table.Key}: {count} records");
            }
            catch (Exception ex)
            {
              System.Diagnostics.Debug.WriteLine($"{table.Key}: ERROR - {ex.Message}");
            }
          }

          System.Diagnostics.Debug.WriteLine("=== End Integrity Check ===");
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error during integrity check: {ex.Message}");
      }
    }
  }
}