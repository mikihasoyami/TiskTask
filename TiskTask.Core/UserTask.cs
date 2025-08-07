using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public int Id { get; private set; }

    /// <summary>
    /// Идентификатор пользователя telegram.
    /// </summary>
    public int UserId { get; private set; }

    /// <summary>
    /// Название задачи.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Описание задачи.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Дата создания задачи.
    /// </summary>
    public DateTime Created { get; private set; }

    /// <summary>
    /// Потраченное время на задачу.
    /// </summary>
    public TimeSpan TimeSpent {  get; private set; }
    #endregion
  }
}
