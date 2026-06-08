using System;
using System.Threading;
using System.Threading.Tasks;

namespace TiskTask.TelegramBot
{
  /// <summary>
  /// Консольное приложение для запуска Телеграм бота.
  /// </summary>
  class Program
  {
    static async Task Main(string[] args)
    {
      Console.WriteLine("Запуск Telegram-бота...");

      var cts = new CancellationTokenSource();
      ConfigManager configManager = new ConfigManager();
      string readerFile = configManager.GetToken();

      if (string.IsNullOrWhiteSpace(readerFile))
      {
        Console.WriteLine("Токен не задан. Запуск Telegram-бота пропущен.");
        return;
      }

      try
      {
        var botService = new TelegramBot(readerFile);
        await botService.StartAsync(cts.Token);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Не удалось запустить Telegram-бота: " + ex.Message);
        Console.WriteLine("Запуск без бота.");
      }
    }
  }
}
