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
    }

    #endregion
  }

  /// <summary>
  /// класс счетчика
  /// </summary>
  public class Stopwatch
  { 
    public DateTime StartTime { get; } //берет время запуска

    public Stopwatch()
    {
      StartTime = DateTime.Now; //при создании устанавливает
    }
    

    public Stop() //при остановке считает разницу и выводить
    {
      TimeSpan elapsedTime = DateTime.Now - StartTime;
      string timeData = String.Format("{0:HH ч. mm м. ss с. ff мс.}", elapsedTime);
      Console.WriteLine(timeData);
      return elapsedTime;
    }
  }
}
