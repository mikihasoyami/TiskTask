using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiskTask.Core;

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
/// Идентификатор пользователя telegram.
/// </summary>
public long UserId { get; set; }

/// <summary>
/// Название задачи.
/// </summary>
public string Title { get; set; }

/// <summary>
/// Описание задачи.
/// </summary>
public string Description { get; set; }

/// <summary>
/// Дата создания задачи.
/// </summary>
public DateTime Created { get; set; }

/// <summary>
/// Потраченное время на задачу.
/// </summary>
public TimeSpan TimeSpent {  get; set; }
#endregion

#region Конструкторы

public UserTask(int id, int userId, string title, string description, DateTime createdDate)
{
  Id = id;
  UserId = userId;
  Title = title;
  Description = description;
  Created = createdDate;
}

public UserTask() 
{
  Id = -1;
  UserId = -1;
  Title = "None";
  Description = "None";
  Created = DateTime.Now;
}
#endregion
}
