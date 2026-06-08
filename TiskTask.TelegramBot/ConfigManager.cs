using System;
using System.IO;

namespace TiskTask.TelegramBot
{
  internal class ConfigManager
  {
    #region Поля и свойства
    /// <summary>
    /// Поле для сохранения пути к файлу с токеном.
    /// </summary>
    private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.txt");
    #endregion

    #region Методы
    /// <summary>
    /// Метод по получению токена из файла.
    /// </summary>
    public string GetToken()
    {
      FileInfo fileInfo = new FileInfo(_filePath);
      string readerToken = string.Empty;

      try
      {
        if (File.Exists(_filePath))
        {
          using StreamReader reader = fileInfo.OpenText();
          readerToken = reader.ReadLine() ?? string.Empty;
        }
        else
        {
          Console.WriteLine("Файл не найден.");
          File.Create(_filePath).Close();
          Console.Write("Файл создан автоматически по пути ");
          Console.Write(fileInfo + "\n");
          Console.WriteLine("Добавьте токен в файл, если захотите запускать Telegram-бота.");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Ошибка при чтении файла: " + ex.Message);
      }

      return readerToken;
    }

    /// <summary>
    /// Метод по удалению файла с токеном.
    /// </summary>
    public void DeletFile()
    {
      if (File.Exists(_filePath))
      {
        File.Delete(_filePath);
      }
    }
    #endregion
  }
}
