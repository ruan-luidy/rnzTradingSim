public static class GameConstants
{
  public const decimal MIN_BET = 0.01m;
  public const decimal MAX_BET = 1_000_000m;
  public const decimal DEFAULT_BALANCE = 10_000m;
  public const decimal MAX_PAYOUT = 2_000_000m;

  // House edge para diferentes jogos
  public const decimal MINES_HOUSE_EDGE = 0.97m; // 3% house edge
  public const decimal COINFLIP_HOUSE_EDGE = 0.95m; // 5% house edge
}