using System.Windows;
using rnzTradingSim.Services;

namespace rnzTradingSim;

public partial class App : Application
{
  protected override void OnStartup(StartupEventArgs e)
  {
    base.OnStartup(e);

    try
    {
      // Inicializar o banco de dados na inicialização da aplicação
      DatabaseInitializer.Initialize();

      // Verificar integridade após inicialização
      DatabaseInitializer.VerifyDatabaseIntegrity();

      // TEMPORÁRIO: Forçar recriação das moedas se não existirem
      DatabaseInitializer.RecreateCoins();
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Critical error during application startup: {ex.Message}");

      // Mostrar mensagem de erro para o usuário
      MessageBox.Show(
        $"Erro ao inicializar o banco de dados:\n{ex.Message}\n\nTentando recriar o banco...",
        "Erro de Inicialização",
        MessageBoxButton.OK,
        MessageBoxImage.Warning
      );

      try
      {
        // Tentar resetar tudo como último recurso
        DatabaseInitializer.ResetAllData();

        MessageBox.Show(
          "Banco de dados recriado com sucesso!",
          "Sucesso",
          MessageBoxButton.OK,
          MessageBoxImage.Information
        );
      }
      catch (Exception resetEx)
      {
        MessageBox.Show(
          $"Falha crítica ao inicializar o banco:\n{resetEx.Message}\n\nA aplicação será fechada.",
          "Erro Crítico",
          MessageBoxButton.OK,
          MessageBoxImage.Error
        );

        Current.Shutdown(-1);
        return;
      }
    }
  }

  protected override void OnExit(ExitEventArgs e)
  {
    // Cleanup if needed
    base.OnExit(e);
  }
}