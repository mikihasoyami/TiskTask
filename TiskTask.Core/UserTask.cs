using System;

namespace TiskTask.Core
{
  /// <summary>
  /// Описывает модель задачи.
  /// </summary>
  public class UserTask
  {
    #region Поля и свойства

    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Название задачи.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание задачи.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Дата создания задачи.
    /// </summary>
    public DateTime Created { get; init; }

    /// <summary>
    /// Потраченное время на задачу.
    /// </summary>
    public TimeSpan TimeSpent { get; set; }

    /// <summary>
    /// Признак того, что задача сейчас активна.
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Момент последнего запуска таймера.
    /// </summary>
    public DateTime? StartedAtUtc { get; set; }

    /// <summary>
    /// Признак того, что задача завершена.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Статус.
    /// </summary>
    public UserTaskStatus Status { get; set; } = UserTaskStatus.New;

    /// <summary>
    /// Момент завершения задачи.
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    #endregion

    #region Конструкторы

    public UserTask(int id, int userId, string title, string description, DateTime createdDate)
    {
      Id = id;
      UserId = userId;
      Title = title;
      Description = description;
      Created = createdDate;
      TimeSpent = TimeSpan.Zero;
    }

    public UserTask(int userId, string title, string description)
    {
      UserId = userId;
      Title = title;
      Description = description;
      Created = DateTime.UtcNow;
      TimeSpent = TimeSpan.Zero;
    }

    public UserTask()
    {
      UserId = -1;
      Title = "None";
      Description = "None";
      Created = DateTime.Now;
      TimeSpent = TimeSpan.Zero;
    }

    #endregion

    #region Методы

    /// <summary>
    /// Возвращает полное затраченное время с учетом текущего запущенного интервала.
    /// </summary>
    public TimeSpan GetCurrentTimeSpent(DateTime? currentUtc = null)
    {
      if (!IsRunning || StartedAtUtc == null)
      {
        return TimeSpent;
      }

      var now = currentUtc ?? DateTime.UtcNow;
      var elapsed = now - StartedAtUtc.Value;

      if (elapsed < TimeSpan.Zero)
      {
        elapsed = TimeSpan.Zero;
      }

      return TimeSpent + elapsed;
    }

    public void Print()
    {
      Console.WriteLine($" {Title} ({Description})");
    }

    #endregion
  }
}