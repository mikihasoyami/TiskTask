using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TiskTask.Core;
using TiskTask.Model;

namespace TiskTask.Core;

/// <summary>
/// Класс для работы с задачами пользователя через базу данных
/// </summary>
public class UserTaskManager
{
  #region Поля и свойства

  private readonly AppDbContext _context;

  /// <summary>
  /// Секундомер для замеров времени выполнения операций в коде
  /// </summary>
  public Stopwatch Timer { get; private set; }

  #endregion

  #region Конструкторы

  public UserTaskManager(AppDbContext context)
  {
    _context = context ?? throw new ArgumentNullException(nameof(context));
    Timer = new Stopwatch();
  }

  #endregion

  #region Методы управления пользователями

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

    _context.Users.Add(user);
    _context.SaveChanges();

    return user;
  }

  public List<User> GetAllUsers()
  {
    return _context.Users
        .OrderBy(user => user.Name)
        .ThenBy(user => user.Id)
        .ToList();
  }

  public User? GetUserById(long userId)
  {
    return _context.Users.FirstOrDefault(user => user.Id == userId);
  }

  private void EnsureUserExists(long userId)
  {
    var exists = _context.Users.Any(u => u.Id == userId);
    if (!exists)
    {
      var user = new User
      {
        Id = userId,
        Name = $"Пользователь {userId}",
        CreatedAtUtc = DateTime.UtcNow
      };
      _context.Users.Add(user);
      _context.SaveChanges();
    }
  }

  #endregion

  #region Методы управления задачами

  /// <summary>
  /// Получить абсолютно все задачи всех пользователей из базы данных.
  /// </summary>
  public List<UserTask> GetAllTasks()
  {
    return _context.UserTasks.AsNoTracking().ToList();
  }

  public UserTask? GetUserTaskById(int taskId)
  {
    return _context.UserTasks.FirstOrDefault(t => t.Id == taskId);
  }

  /// <summary>
  /// Получить задачи пользователя с возможностью фильтрации по вашему enum
  /// </summary>
  public List<UserTask> GetTasksByUserId(long userId, UserTaskStatus filterStatus = UserTaskStatus.All)
  {
    var query = _context.UserTasks.AsNoTracking().Where(t => t.UserId == userId);

    if (filterStatus != UserTaskStatus.All)
    {
      query = query.Where(t => t.Status == filterStatus);
    }

    return query.OrderByDescending(t => t.Created).ToList();
  }

  public UserTask CreateUserTask(long userId, string title, string description)
  {
    _context.ChangeTracker.Clear();
    EnsureUserExists(userId);

    var newUserTask = new UserTask
    {
      UserId = userId,
      Title = title,
      Description = description,
      Created = DateTime.UtcNow,
      Status = UserTaskStatus.New,
      TimeSpent = TimeSpan.Zero,
      IsRunning = false,
      IsCompleted = false
    };

    _context.UserTasks.Add(newUserTask);

    // 3. Сохраняем задачу
    _context.SaveChanges();

    return newUserTask;
  }


  public bool ChangeUserTask(UserTask updatedTask)
  {
    var existingTask = _context.UserTasks.FirstOrDefault(t => t.Id == updatedTask.Id);
    if (existingTask == null)
    {
      return false;
    }

    existingTask.UserId = updatedTask.UserId;
    existingTask.Title = updatedTask.Title;
    existingTask.Description = updatedTask.Description;
    existingTask.TimeSpent = updatedTask.TimeSpent;
    existingTask.IsRunning = updatedTask.IsRunning;
    existingTask.StartedAtUtc = updatedTask.StartedAtUtc;
    existingTask.IsCompleted = updatedTask.IsCompleted;
    existingTask.CompletedAtUtc = updatedTask.CompletedAtUtc;
    existingTask.Status = updatedTask.Status;

    _context.SaveChanges();
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

      _context.SaveChanges();
    }
  }

  public void DeleteUserTask(int id)
  {
    var removableTask = _context.UserTasks.FirstOrDefault(t => t.Id == id);
    if (removableTask != null)
    {
      _context.UserTasks.Remove(removableTask);
      _context.SaveChanges();
    }
  }

  #endregion

  #region Логика трекинга времени под управления Enum статусами

  /// <summary>
  /// Возвращает активную задачу на основе статуса InProgress
  /// </summary>
  public UserTask? GetActiveTask(long userId)
  {
    return _context.UserTasks
        .FirstOrDefault(t => t.UserId == userId && t.Status == UserTaskStatus.InProgress);
  }

  /// <summary>
  /// Переключает трекер на новую задачу, переводя старую в статус Paused
  /// </summary>
  public UserTask SwitchActiveTask(long userId, int taskId, DateTime? switchedAtUtc = null)
  {
    var switchMomentUtc = switchedAtUtc ?? DateTime.UtcNow;
    var targetTask = _context.UserTasks.FirstOrDefault(t => t.Id == taskId && t.UserId == userId);

    if (targetTask == null)
    {
      throw new KeyNotFoundException($"Задача с Id {taskId} не найдена у пользователя {userId}.");
    }

    if (targetTask.Status == UserTaskStatus.Completed)
    {
      throw new InvalidOperationException($"Задача с Id {taskId} уже завершена.");
    }

    var activeTask = GetActiveTask(userId);

    if (activeTask?.Id == targetTask.Id)
    {
      return activeTask;
    }

    // Приостанавливаем старую задачу (перевод в Paused)
    PauseTaskInternal(activeTask, switchMomentUtc);

    // Запускаем целевую задачу (перевод в InProgress)
    targetTask.Status = UserTaskStatus.InProgress;
    targetTask.IsRunning = true;
    targetTask.StartedAtUtc = switchMomentUtc;
    targetTask.CompletedAtUtc = null;

    _context.SaveChanges();

    return targetTask;
  }

  /// <summary>
  /// Останавливает активную задачу пользователя переводя её в статус Paused
  /// </summary>
  public bool StopActiveTask(long userId, DateTime? stoppedAtUtc = null)
  {
    var activeTask = GetActiveTask(userId);
    if (activeTask == null)
    {
      return false;
    }

    PauseTaskInternal(activeTask, stoppedAtUtc ?? DateTime.UtcNow);
    _context.SaveChanges();
    return true;
  }

  /// <summary>
  /// Завершает задачу окончательно переводя её в статус Completed
  /// </summary>
  public bool CompleteTask(long userId, int taskId, DateTime? completedAtUtc = null)
  {
    var completionMomentUtc = completedAtUtc ?? DateTime.UtcNow;
    var task = _context.UserTasks.FirstOrDefault(t => t.Id == taskId && t.UserId == userId);

    if (task == null || task.Status == UserTaskStatus.Completed) return false;

    // Если задача была запущена, рассчитываем время до момента завершения
    if (task.Status == UserTaskStatus.InProgress)
    {
      PauseTaskInternal(task, completionMomentUtc);
    }

    task.Status = UserTaskStatus.Completed;
    task.IsCompleted = true;
    task.IsRunning = false;
    task.CompletedAtUtc = completionMomentUtc;

    _context.SaveChanges();
    return true;
  }

  public TimeSpan GetTrackedTime(int taskId, DateTime? currentUtc = null)
  {
    var task = GetUserTaskById(taskId);
    return task?.GetCurrentTimeSpent(currentUtc) ?? TimeSpan.Zero;
  }

  /// <summary>
  /// Внутренний метод для перевода задачи в статус Paused и фиксации времени
  /// </summary>
  private void PauseTaskInternal(UserTask? task, DateTime pauseMomentUtc)
  {
    if (task == null || task.Status != UserTaskStatus.InProgress) return;

    task.Status = UserTaskStatus.Paused;
    task.IsRunning = false;

    if (task.StartedAtUtc.HasValue)
    {
      task.TimeSpent += pauseMomentUtc - task.StartedAtUtc.Value;
    }
  }

  #endregion
}
