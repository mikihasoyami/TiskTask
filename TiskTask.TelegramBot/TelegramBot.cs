using System;
using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TiskTask.TelegramBot
{
  /// <summary>
  /// Класс для запуска Телеграм бота по токену из файла.
  /// </summary>
  public class TelegramBot
  {
    #region Поля и свойства
    /// <summary>
    /// Создание клиента для работы с Телеграм ботом.
    /// </summary>
    private readonly ITelegramBotClient _botClient;
    #endregion

    #region Методы
    /// <summary>
    /// Запускает бота и начинает получать обновления.
    /// </summary>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
      var me = await _botClient.GetMe();

      Console.WriteLine($"Бот @{me.Username} запущен. Ожидание сообщений...");

      await _botClient.ReceiveAsync(
        updateHandler: new UpdateHandler(_botClient),
        cancellationToken: cancellationToken
      );
    }
    #endregion

    #region Конструкторы
    public TelegramBot(string botToken)
    {
      _botClient = new TelegramBotClient(botToken);
    }
    #endregion
  }
}
