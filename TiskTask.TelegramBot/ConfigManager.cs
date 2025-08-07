using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiskTask.TelegramBot
{
  internal class ConfigManager
  {
    #region Поля и свойства
    /// <summary>
    /// Поле для сохранения пути к файлу с токеном.
    /// </summary>
    private readonly string _filePath;
    #endregion

    #region Методы
    /// <summary>
    /// Метод по получению токена из файла.
    /// </summary>
    public string GetToken()
    {
      string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.txt");
      FileInfo fileInfo = new FileInfo(_filePath);
      string readerToken = "";

      try
      {
        if (File.Exists(_filePath))
        {
          using (StreamReader reader = fileInfo.OpenText())
          {
            readerToken = reader.ReadLine();
          }
        }
        else
        {
          Console.WriteLine("Файл не найден.");
          File.Create(_filePath).Close();
          Console.Write("Файл создан автоматически по пути ");
          Console.Write(fileInfo + "\n");
          Console.WriteLine("Введите токен для вашего телеграм бота: ");
          readerToken = Console.ReadLine();

          using (StreamWriter writer = fileInfo.AppendText())
          {
            writer.WriteLine(readerToken);
          }
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
      File.Delete(_filePath);
    }
    #endregion
  }
}
