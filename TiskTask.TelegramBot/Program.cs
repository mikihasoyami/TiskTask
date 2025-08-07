using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TiskTask.TelegramBot;

namespace TiskTask.TelegramBot
{
  /// <summary>
  /// Консольное приложение для запуска Телеграм бота.
  /// </summary>
  class Program
  {
    static async Task Main(string[] args)
    {
      while (true)
      {
        Console.WriteLine("Запуск Telegram-бота...");

        var cts = new CancellationTokenSource();

        ConfigManager configManager = new ConfigManager();
        string readerFile = configManager.GetToken();

        try
        {
          var botService = new TelegramBot(readerFile);

          await botService.StartAsync(cts.Token);
        }
        catch (Exception ex)
        {
          Console.WriteLine("Ошибка при чтении файла: " + ex.Message);
          Console.WriteLine("Вы ввели не правильный токен, приложение будет перезапущено, " +
            "введите токен правильно");
          configManager.DeletFile();
        }

        Console.WriteLine("Бот остановлен.\n");
      }
    }
  }
}
