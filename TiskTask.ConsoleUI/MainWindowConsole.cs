using System;
using System.Collections.Generic;
using System.Linq;
using TiskTask.Core;

namespace TiskTask.ConsoleUI;

public class MainWindowConsole
{
    private readonly string _login;
    private readonly UserTaskManager _tasksManager;
    private readonly List<string> _menu;
    private int? _activeTaskId;
    private long _userId;

    public MainWindowConsole(UserModel user)
    {
        _login = user.Login;
        _userId = user.Id;
        _tasksManager = new UserTaskManager();
        _activeTaskId = null;
        _menu = new List<string>
        {
            "1. Начать задачу",
            "2. Завершить задачу",
            "3. Добавить задачу",
            "4. Удалить задачу",
            "5. Выход"
        };
    }

    public void Menu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"LOGIN: {_login}");

            if (_activeTaskId.HasValue)
            {
                var activeTask = _tasksManager.GetUserTaskById(_activeTaskId.Value);
                Console.WriteLine($"АКТИВНАЯ ЗАДАЧА: {activeTask.Title}");
            }

            Console.WriteLine("MENU:");
            foreach (var item in _menu)
            {
                Console.WriteLine(item);
            }
            Console.Write("Выберите пункт меню: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    StartTask();
                    break;
                case "2":
                    StopTask();
                    break;
                case "3":
                    AddTask();
                    break;
                case "4":
                    DeleteTask();
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Неверный выбор! Нажмите любую клавишу...");
                    Console.ReadKey();
                    break;
            }
        }
    }

    private void StartTask()
    {
        Console.Clear();

        if (_activeTaskId.HasValue)
        {
            StopTask();
        }

        if (_tasksManager.UsersTasks.Count == 0)
        {
            Console.WriteLine("Нет доступных задач. Сначала добавьте задачу.");
            Console.ReadKey();
            return;
        }

        PrintTasks();
        Console.WriteLine();
        Console.Write("Выберите номер задачи: ");

        if (!int.TryParse(Console.ReadLine(), out int taskNumber) ||
            taskNumber < 1 ||
            taskNumber > _tasksManager.UsersTasks.Count)
        {
            Console.WriteLine("Неверный номер задачи!");
            Console.ReadKey();
            return;
        }

        var selectedTask = _tasksManager.UsersTasks[taskNumber - 1];
        _activeTaskId = selectedTask.Id;
        _tasksManager.Timer.Start();

        Console.Clear();
        Console.WriteLine($"Задача '{selectedTask.Title}' начата!");
        Console.WriteLine("Нажмите любую клавишу для возврата в меню...");
        Console.ReadKey();
    }

    private void StopTask()
    {
        Console.Clear();

        if (!_activeTaskId.HasValue)
        {
            Console.WriteLine("Нет активной задачи!");
            Console.ReadKey();
            return;
        }

        var activeTask = _tasksManager.GetUserTaskById(_activeTaskId.Value);

        Console.WriteLine($"Остановка задачи: {activeTask.Title}");
        Console.WriteLine("Нажмите любую клавишу для остановки таймера...");
        Console.ReadKey();

        var elapsedTime = _tasksManager.Timer.Stop();
        activeTask.TimeSpent = activeTask.TimeSpent.Add(elapsedTime);
        _tasksManager.ChangeUserTask(activeTask);

        _activeTaskId = null;

        Console.WriteLine($"\nЗадача завершена!");
        Console.WriteLine($"Затраченное время: {FormatTimeSpan(elapsedTime)}");
        Console.WriteLine($"Общее время на задаче: {FormatTimeSpan(activeTask.TimeSpent)}");
        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    private void AddTask()
    {
        Console.Clear();
        Console.WriteLine("Добавление новой задачи");
        Console.WriteLine(new string('-', 30));

        Console.Write("Введите название задачи: ");
        var title = Console.ReadLine();

        Console.Write("Введите описание задачи: ");
        var description = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(title))
        {
            Console.WriteLine("Название задачи не может быть пустым!");
            Console.ReadKey();
            return;
        }

        try
        {
            var newId = _tasksManager.UsersTasks.Count > 0
                ? _tasksManager.UsersTasks.Max(t => t.Id) + 1
                : 1;

            var newTask = _tasksManager.CreateUserTask(newId, _userId, title, description);
            Console.WriteLine($"\n✅ Задача '{newTask.Title}' успешно добавлена!");
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

        if (_tasksManager.UsersTasks.Count == 0)
        {
            Console.WriteLine("Нет задач для удаления.");
            Console.ReadKey();
            return;
        }

        PrintTasks();
        Console.WriteLine();
        Console.Write("Выберите номер задачи для удаления: ");

        if (!int.TryParse(Console.ReadLine(), out int taskNumber) ||
            taskNumber < 1 ||
            taskNumber > _tasksManager.UsersTasks.Count)
        {
            Console.WriteLine("Неверный номер задачи!");
            Console.ReadKey();
            return;
        }

        var taskToDelete = _tasksManager.UsersTasks[taskNumber - 1];

        if (_activeTaskId == taskToDelete.Id)
        {
            Console.WriteLine("Нельзя удалить активную задачу! Сначала завершите её.");
            Console.ReadKey();
            return;
        }

        Console.Write($"Вы уверены, что хотите удалить задачу '{taskToDelete.Title}'? (y/n): ");
        var confirm = Console.ReadLine();

        if (confirm?.ToLower() == "y")
        {
            _tasksManager.DeleteUserTask(_userId, taskToDelete.Id);
            Console.WriteLine("✅ Задача успешно удалена!");
        }
        else
        {
            Console.WriteLine("Удаление отменено.");
        }

        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    private void PrintTasks()
    {
        if (_tasksManager.UsersTasks.Count == 0)
        {
            Console.WriteLine("Список задач пуст.");
            return;
        }

        Console.WriteLine("Список задач:");
        Console.WriteLine(new string('-', 50));

        for (int i = 0; i < _tasksManager.UsersTasks.Count; i++)
        {
            var task = _tasksManager.UsersTasks[i];
            Console.Write($"{i + 1}. ");
            task.Print();
            Console.WriteLine($"   Время: {FormatTimeSpan(task.TimeSpent)}");
        }

        Console.WriteLine(new string('-', 50));
    }

    private string FormatTimeSpan(TimeSpan time)
    {
        if (time.TotalHours >= 1)
            return $"{time.Hours}ч {time.Minutes}м {time.Seconds}с";
        else if (time.TotalMinutes >= 1)
            return $"{time.Minutes}м {time.Seconds}с";
        else
            return $"{time.Seconds}с";
    }
}