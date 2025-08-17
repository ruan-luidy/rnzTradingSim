public static class GameConstants
{
  public const decimal MIN_BET = 0.01m;
  public const decimal MAX_BET = 1_000_000m;
  public const decimal DEFAULT_BALANCE = 10_000m;
  public const decimal MAX_PAYOUT = 2_000_000m;

  // House edge para diferentes jogos
  public const decimal MINES_HOUSE_EDGE = 0.97m; // 3% house edge
  public const decimal COINFLIP_HOUSE_EDGE = 0.95m; // 5% house edge

  // Configurações de moeda
  public const string CURRENCY_SYMBOL = "$";
  public const string CURRENCY_CODE = "USD";
}

public static class UIConstants
{
  // Timers e intervalos
  public const int MARKET_UPDATE_INTERVAL_MS = 2 * 60 * 1000; // 2 minutos
  public const int UI_REFRESH_INTERVAL_MS = 1000; // 1 segundo
  public const int ANIMATION_DURATION_MS = 300;

  // Paginação
  public const int DEFAULT_PAGE_SIZE = 12;
  public const int MAX_PAGE_SIZE = 50;

  // Cache
  public const int COIN_CACHE_DURATION_MINUTES = 5;
  public const int PLAYER_STATS_CACHE_DURATION_MINUTES = 1;
}

public static class DatabaseConstants
{
  // Limpeza de dados
  public const int GAME_HISTORY_RETENTION_DAYS = 30;
  public const int CLEANUP_FREQUENCY_GAMES = 100;
  public const int MAX_GAME_RESULTS_PER_PLAYER = 1000;

  // Performance
  public const int BATCH_SIZE = 100;
  public const int QUERY_TIMEOUT_SECONDS = 30;

  // Logs
  public const int LOG_RETENTION_DAYS = 7;
}

public static class NetworkConstants
{
  // Simulação de rede
  public const int MIN_NETWORK_DELAY_MS = 100;
  public const int MAX_NETWORK_DELAY_MS = 500;
  public const int NETWORK_TIMEOUT_MS = 5000;
}