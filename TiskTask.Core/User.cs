using System;
using System.Collections.Generic;
using TiskTask.Core;

namespace TiskTask.Model;

/// <summary>
/// Сущность пользователя в базе данных
/// </summary>
public class User
{
  /// <summary>
  /// Уникальный идентификатор пользователя (например, Telegram ID)
  /// </summary>
  public long Id { get; set; }

  /// <summary>
  /// Имя пользователя или логин
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// Хэшированный пароль для авторизации
  /// </summary>
  public string Password { get; set; } = string.Empty;

  /// <summary>
  /// Дата регистрации
  /// </summary>
  public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
  public bool IsAdmin { get; set; }
  /// <summary>
  /// Навигационное свойство: список задач этого пользователя
  /// </summary>
  public virtual ICollection<UserTask> Tasks { get; set; } = new List<UserTask>();
}
