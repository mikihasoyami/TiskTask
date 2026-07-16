using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TiskTask.Core;
using TiskTask.Model;
using TiskTask.TelegramBot;
using static System.Collections.Specialized.BitVector32;

namespace TiskTask.TelegramBot
{
    /// <summary>
    /// Обработчик входящих обновлений от Telegram.
    /// </summary>
    internal class UpdateHandler : IUpdateHandler
    {
        #region Поля и свойства
        /// <summary>
        /// Создание клиента для работы с Телеграм ботом.
        /// </summary>
        private readonly ITelegramBotClient _botClient;

        /// <summary>
        /// Настройка сериализации JSON.
        /// </summary>
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// Проверяет была ли запущена команда /create
        /// </summary>
        public static bool create;

        /// <summary>
        /// Ассоциативный массив для передачи данных о задаче
        /// </summary>
        public static Dictionary<string, string> taskData = new Dictionary<string, string>();

        public string title = "";
        public string description = "";
        private static readonly AppDbContext _context = new AppDbContext();
        public UserTaskManager userTaskManagerTG = new UserTaskManager(_context);
        #endregion

        #region Методы
        /// <summary>
        /// Отправка текстового сообщения.
        /// </summary>
        /// <param name="chatId">Id пользователя.</param>
        /// <param name="text">Техт пользователя.</param>
        /// <param name="cancellationToken">Токен для отмены операции.</param>
        private async Task SendTextMessageAsync(long chatId, string text, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(chatId: chatId, text: text, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Логирование входящих обновлений от Telegram.
        /// </summary>
        /// <param name="update">Обновления от Телеграм.</param>
        private void LogUpdate(Update update)
        {
            try
            {
                var json = JsonSerializer.Serialize(update, JsonOptions);
                //Console.WriteLine($"Обновление получено: {json}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сериализации: {ex.Message}");
            }
        }
        #endregion

        #region <IUpdateHandler>
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            LogUpdate(update);
            try
            {
                switch (update.Type)
                {
                    case Telegram.Bot.Types.Enums.UpdateType.Message:
                        {
                            if (update.Message is not Message message) return;

                            var chatId = message.Chat.Id;

                            var text = message.Text;

                            if (message.Type == MessageType.Text && !string.IsNullOrEmpty(text))
                            {
                                if (text == BotChatCommands.Start)
                                {
                                    await SendTextMessageAsync(chatId, "🙌🏿 Добро пожаловать!\n\n" +
                                      "Я бот для работы с твоими задачами 😉\n" +
                                      "Благодаря мне ты можешь:\n" +
                                      " * Создавать\n" +
                                      " * Удалять\n" +
                                      " * Редактировать\n" +
                                      " * И засекать время выполнения задачи 😎", cancellationToken);
                                    await botClient.SendMessage(
                                      chatId: chatId,
                                      text: $"Вот список команд для моей работы:\n\n" +
                                      $"{BotChatCommands.Start} - это начало мой работы 🐝\n" +
                                      $"{BotChatCommands.All} - это вывод всех твоих задач 🦅\n" +
                                      $"{BotChatCommands.Create} - это добавление новой задачи 🐙\n",
                                      cancellationToken: cancellationToken
                                    );

                                    using var context = new Core.AppDbContext();

                                    var userTasks = await context.UserTasks
                                      .Where(task => task.UserId == chatId)
                                      .ToListAsync();
                                    var manager = new UserTaskManager(_context);

                                    return;
                                }
                                else if (text == BotChatCommands.All)
                                {
                                    long currentUserId = chatId;
                                    var userTasks = userTaskManagerTG.GetTasksByUserId(currentUserId);
                                    await CommandManager.TakeAllTasksCommand(botClient, chatId, cancellationToken, userTasks);
                                }

                                else if (text == BotChatCommands.Create)
                                {
                                    create = true;
                                    await CommandManager.RequestTaskDescriptionAsync(botClient, update);

                                }
                                else if ((text != BotChatCommands.Create) && (create == true))
                                {
                                    long userId = chatId;
                                    await CommandManager.CreateTaskAsync(botClient, chatId, update);
                                    userTaskManagerTG.CreateUserTask(userId, taskData["title"], taskData["description"]);
                                    create = false;
                                }
                                else
                                {
                                    await botClient.SendMessage(
                                      chatId: chatId,
                                      text: $"Вот список команд для моей работы:\n\n" +
                                      $"{BotChatCommands.Start} - это начало мой работы 🐝\n" +
                                      $"{BotChatCommands.All} - это вывод всех твоих задач 🦅\n" +
                                      $"{BotChatCommands.Create} - это добавление новой задачи 🐙\n",
                                      cancellationToken: cancellationToken
                                    );
                                }
                            }
                        }
                        return;

                    case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                      {
                        var callbackQuery = update.CallbackQuery;
                        if (callbackQuery == null || string.IsNullOrWhiteSpace(callbackQuery.Data))
                        {
                          return;
                        }

                        var user = callbackQuery.From;
                        var chat = callbackQuery.Message?.Chat;
                        if (chat == null)
                        {
                          return;
                        }

                        var tryBotton = callbackQuery.Data;
                        var parse = tryBotton.Split('_');

                        if (parse.Length < 2 || !int.TryParse(parse[0], out int IdTask))
                        {
                          await botClient.AnswerCallbackQuery(callbackQuery.Id, "Ошибка: неверный формат данных кнопки.");
                          return;
                        }

                        var action = parse[1];

                        switch (action)
                        {
                          case "start":
                            Console.WriteLine("start");
                            break;

                          case "stop":
                            Console.WriteLine("stop");
                            break;

                          case "edit":
                            Console.WriteLine("edit");
                            UserTask? userTask = userTaskManagerTG.GetUserTaskById(IdTask);

                            if (userTask == null)
                            {
                              await botClient.AnswerCallbackQuery(callbackQuery.Id, "Ошибка: Задача не найдена в базе!");
                              return;
                            }

                            userTaskManagerTG.ChangeUserTask(userTask);
                            break;

                          case "remove":
                            Console.WriteLine("remove");
                            userTaskManagerTG.DeleteUserTask(IdTask);
                            break;
                        }

                        Console.WriteLine(IdTask);
                        Console.WriteLine(action);
                        await botClient.AnswerCallbackQuery(callbackQuery.Id);
                      }
                      return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка при обработке сообщения: {ex}");
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Конструкторы
        public UpdateHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }
        #endregion
    }
}