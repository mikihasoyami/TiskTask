using System;
using System.Collections.Generic;
using System.Text;

namespace TiskTask.ConsoleUI;

public class MainWindowConsole
{
    private string _login;
    private Dictionary<string, Timer> _tasks;
    private List<string> _menu;
    public MainWindowConsole(UserModel user) 
    {
        _login = user.Login;
        _tasks = user.GetTasks();
        _menu = new List<string>
        {
            "1. Начать задачу",
            "2. Добавить задачу",
            "3. Удалить задачу",
            "4. Выход"
        };
    }

    public void Menu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("MENU:");
            foreach (var item in _menu)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("Выберите пункт меню");
            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    Console.Clear();
                    PrintTasks();
                    Console.WriteLine();
                    Console.WriteLine("Выберите задачу");
                    var number = int.Parse(Console.ReadLine());
                    await TaskTimer.Start(number - 1);
                    Console.WriteLine("Задача остановлена!");
                    break;
                case "2":
                    Console.Clear();
                    PrintTasks();
                    Console.WriteLine();
                    Console.WriteLine("Введите название задачи");
                    try
                    {
                        CreateTask(Console.ReadLine());
                        Console.WriteLine("Задача успешно добавлена!");
                    }
                    catch (Exception ex) 
                    {
                        Console.WriteLine(ex); 
                    }
                    break;
                case "3":
                    Console.Clear();
                    PrintTasks();
                    Console.WriteLine();
                    Console.WriteLine("Выберите задачу");
                    try
                    {
                        DeleteTask(int.Parse(Console.ReadLine());
                        Console.WriteLine("Задача успешно удалена!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    break;
                case "4":
                    return;
            }
        }
    }

    private void PrintTasks()
    {
        for (var i=0; i<_tasks.Count; i++)
        {
            Console.WriteLine($"{i+1} Задача '{_tasks.Keys}' время выполнения : {_tasks.Values}");
        }
    }
}
