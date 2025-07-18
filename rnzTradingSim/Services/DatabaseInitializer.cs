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

          // Se não há moedas, criar
          if (!coinsExist)
          {
            System.Diagnostics.Debug.WriteLine("No coins found, creating fake coins...");
            var coinService = new FakeCoinService(context);
            coinService.RecreateAllCoins();
          }
        }

        // Verificar o serviço de moedas fake
        using (var coinContext = new TradingDbContext())
        {
          var coinService = new FakeCoinService(coinContext);
          var coinCount = coinService.GetTotalCoinsCount();
          System.Diagnostics.Debug.WriteLine($"Total coins available: {coinCount}");

          // Se ainda não há moedas, forçar criação
          if (coinCount == 0)
          {
            System.Diagnostics.Debug.WriteLine("Force creating coins...");
            coinService.RecreateAllCoins();
            coinCount = coinService.GetTotalCoinsCount();
            System.Diagnostics.Debug.WriteLine($"After force creation - Total coins: {coinCount}");
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

            // Criar moedas após recriar o banco
            var coinService = new FakeCoinService(context);
            coinService.RecreateAllCoins();
            System.Diagnostics.Debug.WriteLine("Coins created after database recreation");
          }

          // Verificar novamente
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

          // Criar moedas fake imediatamente após recriar
          var coinService = new FakeCoinService(context);
          coinService.RecreateAllCoins();
          System.Diagnostics.Debug.WriteLine("Fake coins created after reset");
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

    // Método específico para recriar apenas as moedas
    public static void RecreateCoins()
    {
      try
      {
        System.Diagnostics.Debug.WriteLine("Recreating coins only...");

        using (var context = new TradingDbContext())
        {
          var coinService = new FakeCoinService(context);
          coinService.RecreateAllCoins();

          var coinCount = coinService.GetTotalCoinsCount();
          System.Diagnostics.Debug.WriteLine($"Recreated {coinCount} coins successfully!");
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error recreating coins: {ex.Message}");
        throw;
      }
    }
  }
}