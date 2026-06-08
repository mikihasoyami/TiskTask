using TiskTask.Core;

const long DemoUserId = 424242;

var manager = new UserTaskManager();

Console.WriteLine("TiskTask Console Demo");
Console.WriteLine("Здесь можно создавать задачи, запускать одну активную задачу и переключаться между ними.");

while (true)
{
    manager.ReloadFromDatabase();

    Console.WriteLine();
    ShowActiveTask(manager);
    Console.WriteLine();
    Console.WriteLine("Меню:");
    Console.WriteLine("1 - Показать задачи");
    Console.WriteLine("2 - Создать задачу");
    Console.WriteLine("3 - Запустить или переключить задачу");
    Console.WriteLine("4 - Остановить активную задачу");
    Console.WriteLine("5 - Удалить задачу");
    Console.WriteLine("6 - Выйти");
    Console.Write("Выбери действие: ");

    var command = Console.ReadLine()?.Trim();
    Console.WriteLine();

    switch (command)
    {
        case "1":
            ShowTasks(manager);
            break;
        case "2":
            CreateTask(manager);
            break;
        case "3":
            SwitchTask(manager);
            break;
        case "4":
            StopTask(manager);
            break;
        case "5":
            DeleteTask(manager);
            break;
        case "6":
            Console.WriteLine("Выход из ConsoleDemo.");
            return;
        default:
            Console.WriteLine("Неизвестная команда. Введи число от 1 до 6.");
            break;
    }
}

static void ShowActiveTask(UserTaskManager manager)
{
    var activeTask = manager.GetActiveTask(DemoUserId);
    if (activeTask == null)
    {
        Console.WriteLine("Активная задача: нет");
        return;
    }

    var trackedTime = manager.GetTrackedTime(activeTask.Id);
    Console.WriteLine($"Активная задача: [{activeTask.Id}] {activeTask.Title} ({trackedTime:hh\\:mm\\:ss})");
}

static void ShowTasks(UserTaskManager manager)
{
    var tasks = manager.GetAllUserTasks(DemoUserId)
        .OrderBy(task => task.Id)
        .ToList();

    if (tasks.Count == 0)
    {
        Console.WriteLine("Задач пока нет. Сначала создай хотя бы одну.");
        return;
    }

    Console.WriteLine("Список задач:");
    Console.WriteLine();

    foreach (var task in tasks)
    {
        var trackedTime = manager.GetTrackedTime(task.Id);
        var status = task.IsRunning ? "В работе" : "На паузе";

        Console.WriteLine($"[{task.Id}] {task.Title}");
        Console.WriteLine($"  Описание: {task.Description}");
        Console.WriteLine($"  Статус: {status}");
        Console.WriteLine($"  Время: {trackedTime:hh\\:mm\\:ss}");
        Console.WriteLine();
    }
}

static void CreateTask(UserTaskManager manager)
{
    Console.Write("Название задачи: ");
    var title = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(title))
    {
        Console.WriteLine("Название не может быть пустым.");
        return;
    }

    Console.Write("Описание задачи: ");
    var description = Console.ReadLine()?.Trim() ?? string.Empty;

    var task = manager.CreateUserTask(DemoUserId, title, description);
    Console.WriteLine($"Задача создана. Id = {task.Id}");
}

static void SwitchTask(UserTaskManager manager)
{
    var tasks = manager.GetAllUserTasks(DemoUserId).ToList();
    if (tasks.Count == 0)
    {
        Console.WriteLine("Нет задач для запуска. Сначала создай задачу.");
        return;
    }

    ShowTasks(manager);
    Console.Write("Введи Id задачи, на которую нужно переключиться: ");
    var rawTaskId = Console.ReadLine();

    if (!int.TryParse(rawTaskId, out var taskId))
    {
        Console.WriteLine("Id задачи должен быть числом.");
        return;
    }

    try
    {
        var task = manager.SwitchActiveTask(DemoUserId, taskId);
        Console.WriteLine($"Теперь активна задача [{task.Id}] {task.Title}.");
    }
    catch (KeyNotFoundException ex)
    {
        Console.WriteLine(ex.Message);
    }
}

static void StopTask(UserTaskManager manager)
{
    var activeTask = manager.GetActiveTask(DemoUserId);
    if (activeTask == null)
    {
        Console.WriteLine("Сейчас нет активной задачи.");
        return;
    }

    manager.StopActiveTask(DemoUserId);
    Console.WriteLine($"Задача [{activeTask.Id}] {activeTask.Title} переведена на паузу.");
}

static void DeleteTask(UserTaskManager manager)
{
    var tasks = manager.GetAllUserTasks(DemoUserId).ToList();
    if (tasks.Count == 0)
    {
        Console.WriteLine("Удалять нечего. Сначала создай задачу.");
        return;
    }

    ShowTasks(manager);
    Console.Write("Введи Id задачи, которую нужно удалить: ");
    var rawTaskId = Console.ReadLine();

    if (!int.TryParse(rawTaskId, out var taskId))
    {
        Console.WriteLine("Id задачи должен быть числом.");
        return;
    }

    var task = tasks.FirstOrDefault(item => item.Id == taskId);
    if (task == null)
    {
        Console.WriteLine($"Задача с Id {taskId} не найдена.");
        return;
    }

    manager.DeleteUserTask(taskId);
    Console.WriteLine($"Задача [{task.Id}] {task.Title} удалена.");
}
