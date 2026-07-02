using System;
using System.Collections.Generic;

namespace TiskTask.Core
{
    /// <summary>
    /// Описывает пользователя системы и его задачи.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Отображаемое имя пользователя.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Дата создания пользователя.
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Список задач пользователя.
        /// </summary>
        public List<UserTask> Tasks { get; set; } = new List<UserTask>();

        public override string ToString()
        {
            return Name;
        }
    }
}
