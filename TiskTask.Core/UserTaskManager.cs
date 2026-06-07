using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TiskTask.Core
{
    /// <summary>
    /// Класс для работы с задачами пользователя
    /// </summary>
    public class UserTaskManager
    {
        #region Поля и свойства

        /// <summary>
        /// Файл со всеми тасками
        /// </summary>
        private readonly UserTaskFileStorage _storage;
        
        /// <summary>
        /// Список задач пользователей
        /// </summary>
        public List<UserTask> UsersTasks { get; set; } = new List<UserTask>();

        #endregion

        #region Методы
        public UserTask CreateUserTask(int id, int userId, string title, string description, DateTime createDate)
        {
            var newUserTask = new UserTask(
                                            id,
                                            userId,
                                            title,
                                            description,
                                            createDate
                                           );
            UsersTasks.Add(newUserTask);
            
            _storage.Save(UsersTasks);
            
            return newUserTask;
        }

        public bool ChangeUserTask(UserTask userTask)
        {
            var existingTask = UsersTasks.FirstOrDefault(t => t.Id == userTask.Id);
            if (existingTask == null)
            {
                return false;
            }
            existingTask.UserId = userTask.UserId;
            existingTask.Title = userTask.Title;
            existingTask.Description = userTask.Description;
            existingTask.Created = userTask.Created;
            existingTask.TimeSpent = userTask.TimeSpent;

            _storage.Save(UsersTasks);
            
            return true;
        }

        public bool DeleteUserTask(int id)
        {
            var removableTask = UsersTasks.FirstOrDefault(t => t.Id == id);
            if (removableTask == null)
            {
                return false;
            }
            UsersTasks.Remove(removableTask);
            
            _storage.Save(UsersTasks);
            
            return true;
        }

        public List<UserTask> GetAllUserTasks(int userId)
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

        #endregion

        #region Конструкторы
        
        public UserTaskManager() 
            : this(new List<UserTask>())
        {
        }

        public UserTaskManager(List<UserTask> userTasks) 
        {
            UsersTasks = userTasks;
        }
        
        public UserTaskManager(UserTaskFileStorage storage)
        {
            _storage = storage;
            UsersTasks = _storage.Load();
        }

        #endregion
    }
}
