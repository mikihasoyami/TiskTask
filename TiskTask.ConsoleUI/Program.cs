namespace TiskTask.ConsoleUI;

public class Program
{
    static void Main()
    {
        string login;
        string password;
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
                Console.WriteLine("Вход");
                Console.WriteLine("Введите логин и пароль:");

                Console.WriteLine("Логин:");
                login = Console.ReadLine();

                Console.WriteLine("Пароль:");
                password = Console.ReadLine();

                // Логика для входа
                if(UsersDB.IsUser(new LoginDto (login, password)))
                {
                    mainWindow(UsersDB.GetUser(login, password).ToModel());
                    mainWindow.Menu();
                    break;
                }
                break;

            case "2":
                Console.WriteLine("Регистрация");
                Console.WriteLine("Введите логин и пароль:");

                Console.WriteLine("Логин:");
                login = Console.ReadLine();

                Console.WriteLine("Пароль:");
                password = Console.ReadLine();

                // Логика для регистрации
                try
                {
                    UsersDB.CreateNewUser(new UserModel(login, password).ToDto());
                    Console.WriteLine("User was successfully created");
                    mainWindow = new MainWindowConsole(UsersDB.GetUser(login, password).ToModel());
                    mainWindow.Menu();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                break;

            case "3":
                return;

            default:
                Console.WriteLine("Неверный выбор. Пожалуйста, выберите пункт меню.");
                break;
        }
    }
}
