using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiskTask.TelegramBot
{
  /// <summary>
  /// Список команд боту при передаче текстовым сообщением.
  /// </summary>
  public static class BotChatCommands
  {
    /// <summary>
    /// Начать взаимодействие с ботом.
    /// </summary>
    public const string Start = "/start";

    /// <summary>
    /// Создать новую сущность (например, событие).
    /// </summary>
    public const string Create = "/create";

    /// <summary>
    /// Показать все элементы (например, всех участников).
    /// </summary>
    public const string All = "/all";
  }
}
