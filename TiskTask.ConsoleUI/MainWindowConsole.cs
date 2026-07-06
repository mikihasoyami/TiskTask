using System;
using System.Collections.Generic;
using System.Linq;
using TiskTask.Core;
using TiskTask.Model;

namespace TiskTask.ConsoleUI;

public class MainWindowConsole
{
  private readonly string _login;
  private readonly UserTaskManager _tasksManager;
  private readonly List<string> _menu;
  private long _userId;

  public MainWindowConsole(User user, UserTaskManager tasksManager)
  {
    _login = user.Name;
    _userId = user.Id;
    _tasksManager = tasksManager ?? throw new ArgumentNullException(nameof(tasksManager));

    _menu = new List<string>
        {
            "1. Управление задачей (Запуск / Переключение)",
            "2. Приостановить активную задачу",
            "3. Завершить задачу окончательно",
            "4. Добавить задачу",
            "5. Удалить задачу",
            "6. Выход"
        };
  }

  public void Menu()
  {
    while (true)
    {
      Console.Clear();
      Console.WriteLine($"LOGIN: {_login}");

      var activeTask = _tasksManager.GetActiveTask(_userId);
      if (activeTask != null)
      {
        Console.WriteLine($"АКТИВНАЯ ЗАДАЧА: {activeTask.Title} (В работе: {FormatTimeSpan(activeTask.GetCurrentTimeSpent())})");
      }
      else
      {
        Console.WriteLine("АКТИВНАЯ ЗАДАЧА: Нет");
      }

      Console.WriteLine("\nMENU:");
      foreach (var item in _menu)
      {
        Console.WriteLine(item);
      }
      Console.Write("Выберите пункт меню: ");
      var choice = Console.ReadLine();

      switch (choice)
      {
        case "1":
          StartOrSwitchTask();
          break;
        case "2":
          PauseTask();
          break;
        case "3":
          CompleteTask();
          break;
        case "4":
          AddTask();
          break;
        case "5":
          DeleteTask();
          break;
        case "6":
          return;
        default:
          Console.WriteLine("Неверный выбор! Нажмите любую клавишу...");
          Console.ReadKey();
          break;
      }
    }
  }

  private void StartOrSwitchTask()
  {
    Console.Clear();
    var userTasks = _tasksManager.GetTasksByUserId(_userId);

    var availableTasks = userTasks.Where(t => t.Status != UserTaskStatus.Completed).ToList();

    if (availableTasks.Count == 0)
    {
      Console.WriteLine("Нет доступных для запуска задач. Сначала добавьте задачу.");
      Console.ReadKey();
      return;
    }

    PrintTasksList(availableTasks);
    Console.WriteLine();
    Console.Write("Выберите номер задачи для запуска: ");

    if (!int.TryParse(Console.ReadLine(), out int taskNumber) || taskNumber < 1 || taskNumber > availableTasks.Count)
    {
      Console.WriteLine("Неверный номер задачи!");
      Console.ReadKey();
      return;
    }

    var selectedTask = availableTasks[taskNumber - 1];

    try
    {
      _tasksManager.SwitchActiveTask(_userId, selectedTask.Id);

      Console.Clear();
      Console.WriteLine($"✅ Задача '{selectedTask.Title}' успешно запущена!");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ Ошибка при переключении задачи: {ex.Message}");
    }

    Console.WriteLine("Нажмите любую клавишу для возврата в меню...");
    Console.ReadKey();
  }

  private void PauseTask()
  {
    Console.Clear();
    bool stopped = _tasksManager.StopActiveTask(_userId);

    if (!stopped)
    {
      Console.WriteLine("У вас нет активной запущенной задачи!");
    }
    else
    {
      Console.WriteLine("✅ Активная задача успешно приостановлена (поставлена на паузу).");
    }

    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
    Console.ReadKey();
  }

  private void CompleteTask()
  {
    Console.Clear();
    var userTasks = _tasksManager.GetTasksByUserId(_userId);
    var activeOrPausedTasks = userTasks.Where(t => t.Status != UserTaskStatus.Completed).ToList();

    if (activeOrPausedTasks.Count == 0)
    {
      Console.WriteLine("Нет задач, которые можно завершить.");
      Console.ReadKey();
      return;
    }

    PrintTasksList(activeOrPausedTasks);
    Console.WriteLine();
    Console.Write("Выберите номер задачи для ОКОНЧАТЕЛЬНОГО завершения: ");

    if (!int.TryParse(Console.ReadLine(), out int taskNumber) || taskNumber < 1 || taskNumber > activeOrPausedTasks.Count)
    {
      Console.WriteLine("Неверный номер задачи!");
      Console.ReadKey();
      return;
    }

    var selectedTask = activeOrPausedTasks[taskNumber - 1];

    _tasksManager.CompleteTask(_userId, selectedTask.Id);

    Console.WriteLine($"\n✅ Задача '{selectedTask.Title}' полностью завершена!");
    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
    Console.ReadKey();
  }

  private void AddTask()
  {
    Console.Clear();
    Console.WriteLine("Добавление новой задачи в базу данных");
    Console.WriteLine(new string('-', 30));

    Console.Write("Введите название задачи: ");
    var title = Console.ReadLine();

    Console.Write("Введите описание задачи: ");
    var description = Console.ReadLine();
    if (description == null)
    {
      description = "";
    }

    if (string.IsNullOrWhiteSpace(title))
    {
      Console.WriteLine("Название задачи не может быть пустым!");
      Console.ReadKey();
      return;
    }

    try
    {
      var newTask = _tasksManager.CreateUserTask(_userId, title, description);
      Console.WriteLine($"\n✅ Задача '{newTask.Title}' успешно добавлена в БД с Id {newTask.Id}!");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"\n❌ Ошибка при добавлении задачи: {ex.Message}");
    }

    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
    Console.ReadKey();
  }

  private void DeleteTask()
  {
    Console.Clear();
    var userTasks = _tasksManager.GetTasksByUserId(_userId);

    if (userTasks.Count == 0)
    {
      Console.WriteLine("Нет задач для удаления.");
      Console.ReadKey();
      return;
    }

    PrintTasksList(userTasks);
    Console.WriteLine();
    Console.Write("Выберите номер задачи для удаления: ");

    if (!int.TryParse(Console.ReadLine(), out int taskNumber) || taskNumber < 1 || taskNumber > userTasks.Count)
    {
      Console.WriteLine("Неверный номер задачи!");
      Console.ReadKey();
      return;
    }

    var taskToDelete = userTasks[taskNumber - 1];

    if (taskToDelete.Status == UserTaskStatus.InProgress)
    {
      Console.WriteLine("Нельзя удалить задачу, которая сейчас находится в работе! Сначала приостановите её.");
      Console.ReadKey();
      return;
    }

    Console.Write($"Вы уверены, что хотите полностью удалить задачу '{taskToDelete.Title}' из БД? (y/n): ");
    var confirm = Console.ReadLine();

    if (confirm?.ToLower() == "y")
    {
      _tasksManager.DeleteUserTask(taskToDelete.Id);
      Console.WriteLine("✅ Задача успешно удалена из базы данных!");
    }
    else
    {
      Console.WriteLine("Удаление отменено.");
    }

    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
    Console.ReadKey();
  }

  // Вспомогательный изолированный метод вывода списков
  private void PrintTasksList(List<UserTask> tasks)
  {
    Console.WriteLine("Список задач:");
    Console.WriteLine(new string('-', 60));

    for (int i = 0; i < tasks.Count; i++)
    {
      var task = tasks[i];
      Console.Write($"{i + 1}. ");

      var currentTime = task.GetCurrentTimeSpent();
      Console.WriteLine($"[{task.Status}] {task.Title} ({task.Description}) — Время: {FormatTimeSpan(currentTime)}");
    }

    Console.WriteLine(new string('-', 60));
  }

  private string FormatTimeSpan(TimeSpan time)
  {
    if (time.TotalHours >= 1)
      return $"{(int)time.TotalHours}ч {time.Minutes}м {time.Seconds}с";
    if (time.TotalMinutes >= 1)
      return $"{time.Minutes}м {time.Seconds}с";

    return $"{time.Seconds}с";
  }
}
