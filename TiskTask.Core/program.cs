using System;

namespace TiskTask.Core;

public class program
{
  public static void Main()
  {
    var storage = new UserTaskFileStorage("tasks.csv");
    var manager = new UserTaskManager(storage);
    manager.CreateUserTask(1,1, "testTask", "descripTest", DateTime.Now );

    Console.WriteLine("Press any key to exit");
  }
}