using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TiskTask.Core
{
    /// <summary>
    /// Сохранение и загрузка задач из CSV.
    /// </summary>
    public class UserTaskFileStorage
    {
        private readonly string _filePath;

        #region Методы

        /// <summary>
        /// Загружает все задачи из файла.
        /// </summary>
        public List<UserTask> Load()
        {
            var result = new List<UserTask>();

            if (!File.Exists(_filePath))
            {
                return result;
            }

            var lines = File.ReadAllLines(_filePath);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(';');

                result.Add(new UserTask
                {
                    Id = int.Parse(parts[0]),
                    UserId = long.Parse(parts[1]),
                    Title = parts[2],
                    Description = parts[3],
                    Created = DateTime.Parse(parts[4], CultureInfo.InvariantCulture),
                    TimeSpent = TimeSpan.Parse(parts[5])
                });
            }

            return result;
        }

        /// <summary>
        /// Полностью перезаписывает файл.
        /// </summary>
        public void Save(IEnumerable<UserTask> tasks)
        {
            var lines = tasks.Select(task =>
                string.Join(";",
                    task.Id,
                    task.UserId,
                    task.Title.Replace(";", ","),
                    task.Description.Replace(";", ","),
                    task.Created.ToString("O"),
                    task.TimeSpent));

            File.WriteAllLines(_filePath, lines);
        }
        
        #endregion

        #region Конструктор

        public UserTaskFileStorage(string filePath)
        {
            _filePath = filePath;

            if (!File.Exists(_filePath))
            {
                File.Create(_filePath).Dispose();
            }
        }

        #endregion

    }
}