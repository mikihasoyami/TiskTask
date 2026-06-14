using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TiskTask.Core;

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

            if (parts.Length < 6)
                continue;

            if (!int.TryParse(parts[0], out var id))
                continue;

            if (!long.TryParse(parts[1], out var userId))
                continue;

            var title = parts[2];
            var description = parts[3];

            if (!DateTime.TryParseExact(parts[4], "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var created))
            {
                if (!DateTime.TryParse(parts[4], CultureInfo.InvariantCulture, DateTimeStyles.None, out created))
                    created = DateTime.Now;
            }

            if (!TimeSpan.TryParse(parts[5], out var timeSpent))
                timeSpent = TimeSpan.Zero;

            result.Add(new UserTask
            {
                Id = id,
                UserId = userId,
                Title = title,
                Description = description,
                Created = created,
                TimeSpent = timeSpent
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