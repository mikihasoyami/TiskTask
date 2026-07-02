using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TiskTask.Core;

/// <summary>
/// Класс для работы с задачами пользователя
/// </summary>
public class UserTaskManager
{
    #region Поля и свойства

    private readonly TelegramBotLibraryContext? _context;

    /// <summary>
    /// Список пользователей.
    /// </summary>
    public List<User> Users { get; private set; } = new List<User>();

    /// <summary>
    /// Файл со всеми тасками
    /// </summary>
    private readonly UserTaskFileStorage _tasks = new UserTaskFileStorage("tasks.csv");

    /// <summary>
    /// Список задач пользователей
    /// </summary>
    public List<UserTask> UsersTasks { get; set; } = new List<UserTask>();
    public Stopwatch Timer;

    #endregion

    #region Конструкторы
    public UserTaskManager() : this(new TelegramBotLibraryContext())
    {
        UsersTasks = _tasks.Load();
        Timer = new Stopwatch();
    }

    public UserTaskManager(List<UserTask> userTasks)
    {
        UsersTasks = userTasks;
        Users = userTasks
                .Select(task => task.UserId)
                .Distinct()
                .OrderBy(userId => userId)
                .Select(userId => new User
                {
                    Id = userId,
                    Name = $"Пользователь {userId}"
                })
                .ToList();
        Timer = new Stopwatch();
    }

    public UserTaskManager(TelegramBotLibraryContext context)
    {
        _context = context;
        Users = _context.Users
            .OrderBy(user => user.Name)
            .ThenBy(user => user.Id)
            .ToList();
        UsersTasks = _context.UserTasks.ToList();
    }

    public UserTaskManager(UserTaskFileStorage storage)
    {
        _tasks = storage;
        UsersTasks = _tasks.Load();
        Timer = new Stopwatch();
    }
    #endregion

    #region Методы
    /// <summary>
    /// Создает задачу с автоназначением идентификатора базой данных.
    /// </summary>
    public UserTask CreateUserTask(long userId, string title, string description)
    {
        EnsureUserExists(userId);

        var newUserTask = new UserTask
        {
            UserId = userId,
            Title = title,
            Description = description,
            Created = DateTime.UtcNow
        };

        _context?.UserTasks.Add(newUserTask);

        UsersTasks.Add(newUserTask);
        SaveChanges();
        _tasks.Save(UsersTasks);
        return newUserTask;
    }

    /// <summary>
    /// Создает задачу для консольного UI.
    /// </summary>
    public UserTask CreateUserTask(int taskId, long userId, string title, string description)
    {
        EnsureUserExists(userId);

        var newUserTask = new UserTask
        {
            Id = taskId,
            UserId = userId,
            Title = title,
            Description = description,
            Created = DateTime.UtcNow
        };

        UsersTasks.Add(newUserTask);
        _tasks.Save(UsersTasks);
        return newUserTask;
    }

    /// <summary>
    /// Создает нового пользователя.
    /// </summary>
    public User CreateUser(string name)
    {
        var normalizedName = name.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException("Имя пользователя не может быть пустым.", nameof(name));
        }

        var user = new User
        {
            Name = normalizedName,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context?.Users.Add(user);
        Users.Add(user);
        SaveChanges();

        return user;
    }

    /// <summary>
    /// Возвращает список всех пользователей.
    /// </summary>
    public List<User> GetAllUsers()
    {
        return Users
            .OrderBy(user => user.Name)
            .ThenBy(user => user.Id)
            .ToList();
    }

    /// <summary>
    /// Возвращает пользователя по идентификатору.
    /// </summary>
    public User? GetUserById(long userId)
    {
        return Users.FirstOrDefault(user => user.Id == userId);
    }

    public bool ChangeUserTask(UserTask userTask)
    {
        var existingTask = UsersTasks.FirstOrDefault(t => t.Id == userTask.Id);
        if (existingTask == null)
        {
            return false;
        }

        bool Changes = false;

        if (existingTask.UserId != userTask.UserId)
        {
            existingTask.UserId = userTask.UserId;
            Changes = true;
        }

        if (existingTask.Title != userTask.Title)
        {
            existingTask.Title = userTask.Title;
            Changes = true;
        }

        if (existingTask.Description != userTask.Description)
        {
            existingTask.Description = userTask.Description;
            Changes = true;
        }

        if (existingTask.TimeSpent != userTask.TimeSpent)
        {
            existingTask.TimeSpent = userTask.TimeSpent;
            Changes = true;
        }

        if (existingTask.IsRunning != userTask.IsRunning)
        {
            existingTask.IsRunning = userTask.IsRunning;
            Changes = true;
        }

        if (existingTask.StartedAtUtc != userTask.StartedAtUtc)
        {
            existingTask.StartedAtUtc = userTask.StartedAtUtc;
            Changes = true;
        }

        if (existingTask.IsCompleted != userTask.IsCompleted)
        {
            existingTask.IsCompleted = userTask.IsCompleted;
            Changes = true;
        }

        if (existingTask.CompletedAtUtc != userTask.CompletedAtUtc)
        {
            existingTask.CompletedAtUtc = userTask.CompletedAtUtc;
            Changes = true;
        }

        if (Changes)
        {
            SaveChanges();
            _tasks.Save(UsersTasks);
        }

        return true;
    }

    public void ChangeUserTask(int taskId, string title, string description, long userId)
    {
        var task = GetUserTaskById(taskId);
        if (task != null)
        {
            task.Title = title;
            task.Description = description;
            task.UserId = userId;
        }
    }

    /// <summary>
    ///Удалить задачу в приложении Winform.
    /// </summary>
    /// <param name="id"></param>
    public void DeleteUserTask(int id)
    {
        var removableTask = UsersTasks.FirstOrDefault(t => t.Id == id);
        if (removableTask == null)
        {
            return;
        }

        _context?.UserTasks.Remove(removableTask);
        UsersTasks.Remove(removableTask);
        SaveChanges();

        _tasks.Save(UsersTasks);

    }
    /// <summary>
    ///Удалить задачу в консольном UI.
    /// </summary>
    /// <param name="id"></param>
    public void DeleteUserTask(long userId, int id)
    {
        var removableTask = UsersTasks.FirstOrDefault(t => t.Id == id);
        if (removableTask == null)
        {
            return;
        }

        Console.WriteLine($"Пользователь с ИД {userId} удалил задачу");
        UsersTasks.Remove(removableTask);

        _tasks.Save(UsersTasks);

    }

    /// <summary>
    /// Возвращает активную задачу пользователя, если она есть.
    /// </summary>
    public UserTask? GetActiveTask(long userId)
    {
        return UsersTasks.FirstOrDefault(t => t.UserId == userId && t.IsRunning && !t.IsCompleted);
    }

    /// <summary>
    /// Запускает указанную задачу и останавливает предыдущую активную задачу пользователя.
    /// Работает по принципу шахматных часов: активной может быть только одна задача.
    /// </summary>
    public UserTask SwitchActiveTask(long userId, int taskId, DateTime? switchedAtUtc = null)
    {
        var switchMomentUtc = switchedAtUtc ?? DateTime.UtcNow;
        var targetTask = UsersTasks.FirstOrDefault(t => t.Id == taskId && t.UserId == userId);

        if (targetTask == null)
        {
            throw new KeyNotFoundException($"Задача с Id {taskId} не найдена у пользователя {userId}.");
        }

        if (targetTask.IsCompleted)
        {
            throw new InvalidOperationException($"Задача с Id {taskId} уже завершена.");
        }

        var activeTask = GetActiveTask(userId);

        if (activeTask?.Id == targetTask.Id)
        {
            return activeTask;
        }

        StopTaskInternal(activeTask, switchMomentUtc);

        targetTask.IsRunning = true;
        targetTask.StartedAtUtc = switchMomentUtc;
        targetTask.CompletedAtUtc = null;
        SaveChanges();

        return targetTask;
    }

    /// <summary>
    /// Останавливает текущую активную задачу пользователя и фиксирует накопленное время.
    /// </summary>
    public bool StopActiveTask(long userId, DateTime? stoppedAtUtc = null)
    {
        var activeTask = GetActiveTask(userId);
        if (activeTask == null)
        {
            return false;
        }

        StopTaskInternal(activeTask, stoppedAtUtc ?? DateTime.UtcNow);
        SaveChanges();
        return true;
    }

    /// <summary>
    /// Возвращает полное время задачи с учетом текущего активного интервала.
    /// </summary>
    public TimeSpan GetTrackedTime(int taskId, DateTime? currentUtc = null)
    {
        var task = GetUserTaskById(taskId);
        return task.GetCurrentTimeSpent(currentUtc);
    }

    /// <summary>
    /// Завершает задачу и фиксирует накопленное время.
    /// </summary>
    public bool CompleteTask(long userId, int taskId, DateTime? completedAtUtc = null)
    {
        var completionMomentUtc = completedAtUtc ?? DateTime.UtcNow;
        var task = UsersTasks.FirstOrDefault(t => t.Id == taskId && t.UserId == userId);

        if (task == null)
        {
            return false;
        }

        StopTaskInternal(task, completionMomentUtc);
        task.IsCompleted = true;
        task.CompletedAtUtc = completionMomentUtc;
        SaveChanges();

        return true;
    }

    /// <summary>
    /// Перезагружает список задач из SQLite.
    /// </summary>
    public void ReloadFromDatabase()
    {
        if (_context == null)
        {
            return;
        }

        Users = _context.Users
            .OrderBy(user => user.Name)
            .ThenBy(user => user.Id)
            .ToList();
        UsersTasks = _context.UserTasks.ToList();
    }

    private static void StopTaskInternal(UserTask? task, DateTime stoppedAtUtc)
    {
        if (task == null || !task.IsRunning || task.StartedAtUtc == null)
        {
            return;
        }

        var elapsed = stoppedAtUtc - task.StartedAtUtc.Value;
        if (elapsed < TimeSpan.Zero)
        {
            elapsed = TimeSpan.Zero;
        }

        task.TimeSpent += elapsed;
        task.IsRunning = false;
        task.StartedAtUtc = null;
    }

    private void SaveChanges()
    {
        _context?.SaveChanges();
    }

    private User EnsureUserExists(long userId)
    {
        var existingUser = GetUserById(userId);
        if (existingUser != null)
        {
            return existingUser;
        }

        var user = new User
        {
            Id = userId,
            Name = $"Пользователь {userId}",
            CreatedAtUtc = DateTime.UtcNow
        };

        _context?.Users.Add(user);
        Users.Add(user);
        SaveChanges();

        return user;
    }


    public List<UserTask> GetAllUserTasks(long userId)
    {
        return UsersTasks
            .Where(t => t.UserId == userId)
            .ToList();
    }

    public UserTask GetUserTaskById(int taskId)
    {
        var gettingTask = UsersTasks.FirstOrDefault(t => t.Id == taskId);
        if (gettingTask == null)
            throw new KeyNotFoundException($"Задача с Id {taskId} не найдена.");

        return gettingTask;
    }

    public void Dispose()
    {
        _tasks.Save(UsersTasks);
    }
    #endregion
}

/// <summary>
/// класс счетчика
/// </summary>
public class Stopwatch
{
    private DateTime _startTime;
    private bool _isRunning;

    public DateTime StartTime
    {
        get

        {
            if (!_isRunning)
                throw new InvalidOperationException("Таймер не запущен");
            return _startTime;
        }
    }

    public Stopwatch()
    {
        _isRunning = false;
    }

    public void Start()
    {
        _startTime = DateTime.Now;
        _isRunning = true;
    }

    public TimeSpan Stop() //при остановке считает разницу и выводить
    {
        if (!_isRunning)
            throw new InvalidOperationException("Таймер не запущен");

        TimeSpan elapsedTime = DateTime.Now - StartTime;
        _isRunning = false;

        string timeData = $"{(int)elapsedTime.TotalHours:00} ч. {elapsedTime.Minutes:00} м. {elapsedTime.Seconds:00} с. {elapsedTime.Milliseconds:000} мс.";
        Console.WriteLine($"Время выполнения: {timeData}");
        return elapsedTime;
    }

    public TimeSpan GetCurrentElapsed()
    {
        if (!_isRunning)
            return TimeSpan.Zero;

        return DateTime.Now - _startTime;
    }
}
