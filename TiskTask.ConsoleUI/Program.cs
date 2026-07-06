using System;
using TiskTask.Core;

namespace TiskTask.ConsoleUI;

public class Program
{
  static void Main()
  {
    using var context = new AppDbContext();

    UserManager userManager = new UserManager(context);
    UserTaskManager tasksManager = new UserTaskManager(context);

    string? login;
    string? password;
    MainWindowConsole mainWindow;

    Console.WriteLine("Добро пожаловать в менеджер задач!");
    Console.WriteLine("Выберите действие:");
    Console.WriteLine("1 - Вход");
    Console.WriteLine("2 - Регистрация");
    Console.WriteLine("3 - Выход");

    var choice = Console.ReadLine();
    switch (choice)
    {
      case "1":
        Console.WriteLine("\n--- Вход ---");

        Console.Write("Введите логин: ");
        login = Console.ReadLine();

        Console.Write("Введите пароль: ");
        password = Console.ReadLine();

        // ЗАЩИТА ОТ NULL: Проверяем, что пользователь не ввел пустоту
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
          Console.WriteLine("Ошибка: Логин и пароль не могут быть пустыми!");
          Console.ReadKey();
          return;
        }

        // Пытаемся авторизовать пользователя
        if (userManager.IsUser(login, password))
        {
          var user = userManager.GetUser(login, password);

          if (user == null)
          {
            Console.WriteLine("Ошибка: Не удалось загрузить данные пользователя.");
            Console.ReadKey();
            return;
          }

          mainWindow = new MainWindowConsole(user, tasksManager);
          mainWindow.Menu();
        }
        else
        {
          Console.WriteLine("Неверный логин или пароль!");
          Console.ReadKey();
        }
        break;

      case "2":
        Console.WriteLine("\n--- Регистрация ---");

        Console.Write("Введите логин: ");
        login = Console.ReadLine();

        Console.Write("Введите пароль: ");
        password = Console.ReadLine();

        // ЗАЩИТА ОТ NULL: Проверяем пустой ввод при регистрации
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
          Console.WriteLine("Ошибка: Логин и пароль не могут быть пустыми!");
          Console.ReadKey();
          return;
        }

        try
        {
          userManager.CreateNewUser(login, password);
          Console.WriteLine("=====================================");
          Console.WriteLine("🎉 Пользователь успешно зарегистрирован!");
          Console.WriteLine("=====================================");

          var user = userManager.GetUser(login, password);

          if (user == null)
          {
            Console.WriteLine("Системная ошибка: Пользователь создан, но не найден в базе данных.");
            Console.ReadKey();
            return;
          }

          mainWindow = new MainWindowConsole(user, tasksManager);
          mainWindow.Menu();
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Ошибка при регистрации: {ex.Message}");
          Console.ReadKey();
        }
        break;

      case "3":
        return;

      default:
        Console.WriteLine("Неверный выбор. Программа завершает работу.");
        Console.ReadKey();
        break;
    }
  }
}
