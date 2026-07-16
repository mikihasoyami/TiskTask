using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TiskTask.Core;

namespace TiskTask.TelegramBot;

public class CommandManager
{
    private static readonly AppDbContext _context = new AppDbContext();
    private static readonly UserTaskManager _userTaskManager = new UserTaskManager(_context);

    /// <summary>
    /// Метод для обработки команды /all
    /// </summary>
    /// <param name="botClient">TG Bot API клиента.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="cancellationToken">Прерывание запроса.</param>
    public static async Task TakeAllTasksCommand(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken, List<UserTask> tasks)
    {
        //var tasks = _taskManager.GetAllTasks();

        if (!tasks.Any())
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: "Опаньки, а задач-то нет.",
                cancellationToken: cancellationToken
            );
            return;
        }
        foreach (var task in tasks)
        {
            string message = $"ID: {task.Id}\nЗаголовок: {task.Title}\nОписание: {task.Description}";
            //Добавила кнопку редактирования к описанию задачи
            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(new[]{
        new[]
        {
          InlineKeyboardButton.WithCallbackData("✅ Старт", $"start_{task.Id}"),
          InlineKeyboardButton.WithCallbackData("🛑 Стоп", $"stop_{task.Id}")
        },
        new [] // first row
        {
            InlineKeyboardButton.WithCallbackData("✏️ Редактировать", $"edit_{task.Id}"),
            InlineKeyboardButton.WithCallbackData("❌ Удалить", $"remove_{task.Id}")
        }
        });
            await botClient.SendMessage(
                chatId: chatId,
                text: message,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken
            );
        }
    }
    /// <summary>
    /// Обрабатывает команду /create, отправляет запрос пользователю.
    /// </summary>
    /// <param name="botClient">Идентификатор чата.</param>
    /// <param name="update"></param>
    /// <returns></returns>
    public static async Task RequestTaskDescriptionAsync(ITelegramBotClient botClient, Update update)
    {
        if (update.Message == null || string.IsNullOrWhiteSpace(update.Message.Text))
        {
          return;
        }
        var newChatId = update.Message.Chat.Id;
        var message = update.Message;
        if (message.Text == "/create")
        {
            await botClient.SendMessage(
            chatId: newChatId,
            text: "Введите данные задачи в формате: Заголовок: <текст>; Описание: <текст>");
        }
    }

    /// <summary>
    /// Обрабатывает команду /create,распарсивает ввод пользователя, вызывает метод создания задачи. 
    /// </summary>
    /// <param name="botClient">Идентификатор чата.</param>
    /// <param name="update"></param>
    /// <returns></returns>
    public static async Task CreateTaskAsync(ITelegramBotClient botClient, long chatId, Update update)
    {
        if (update.Message == null || string.IsNullOrWhiteSpace(update.Message.Text))
        {
          return;
        }
        var newChatId = update.Message.Chat.Id;
        var message = update.Message;
        try
        {
            string taskTitle;
            string taskDiscription;
            string userText = message.Text.ToString();
            string[] taskText = userText.Split(";");
            string[] title = taskText[0].Split(":");
            string[] description = taskText[1].Split(":");
            if ((title[0] == "Заголовок") && (description[0] == " Описание"))
            {
                taskTitle = title[1];
                taskDiscription = description[1];
                UpdateHandler.taskData.Add("title", taskTitle);
                UpdateHandler.taskData.Add("description", taskDiscription);

            }
            else
            {
                await botClient.SendMessage(
                chatId: newChatId,
                text: "Неверный формат данных");
            }
        }
        catch (IndexOutOfRangeException)
        {
            await botClient.SendMessage(
            chatId: newChatId,
            text: "Неверный формат данных");
        }
    }
}
