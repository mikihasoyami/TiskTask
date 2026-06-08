namespace TiskTask.WinForms;

static class Program
{
    /// <summary>
    ///  Основная точка входа в приложение.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // настроить параметры приложения
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }    
}