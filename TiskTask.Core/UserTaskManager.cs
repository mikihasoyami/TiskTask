﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TiskTask.Core
{
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
        /// Список задач пользователей
        /// </summary>
        public List<UserTask> UsersTasks { get; set; } = new List<UserTask>();

        #endregion

        #region Методы
        /// <summary>
        /// Создает задачу и при наличии контекста сразу сохраняет ее в SQLite.
        /// </summary>
        public UserTask CreateUserTask(int id, long userId, string title, string description, DateTime createDate)
        {
            EnsureUserExists(userId);

            var newUserTask = new UserTask(
                                            id,
                                            userId,
                                            title,
                                            description,
                                            createDate
                                           );

            _context?.UserTasks.Add(newUserTask);
            UsersTasks.Add(newUserTask);
            SaveChanges();
            return newUserTask;
        }

        /// <summary>
        /// Создает задачу с автоназначением идентификатора базой данных.
        /// </summary>
        public UserTask CreateUserTask(long userId, string title, string description, DateTime? createDate = null)
        {
            EnsureUserExists(userId);

            var newUserTask = new UserTask
            {
                UserId = userId,
                Title = title,
                Description = description,
                Created = createDate ?? DateTime.UtcNow
            };

            _context?.UserTasks.Add(newUserTask);
            UsersTasks.Add(newUserTask);
            SaveChanges();
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
            existingTask.UserId = userTask.UserId;
            existingTask.Title = userTask.Title;
            existingTask.Description = userTask.Description;
            existingTask.Created = userTask.Created;
            existingTask.TimeSpent = userTask.TimeSpent;
            existingTask.IsRunning = userTask.IsRunning;
            existingTask.StartedAtUtc = userTask.StartedAtUtc;
            existingTask.IsCompleted = userTask.IsCompleted;
            existingTask.CompletedAtUtc = userTask.CompletedAtUtc;
            SaveChanges();
            return true;
        }

        public bool DeleteUserTask(int id)
        {
            var removableTask = UsersTasks.FirstOrDefault(t => t.Id == id);
            if (removableTask == null)
            {
                return false;
            }

            _context?.UserTasks.Remove(removableTask);
            UsersTasks.Remove(removableTask);
            SaveChanges();
            return true;
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

        #endregion

        #region Конструкторы
        public UserTaskManager()
            : this(new TelegramBotLibraryContext())
        {
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

        #endregion
    }
}
