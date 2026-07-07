using TiskTask.Core;

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
        using var context = new AppDbContext();

        // 2. Сначала открываем окно авторизации
        using var authForm = new AuthForm(context);

        if (authForm.ShowDialog() == DialogResult.OK)
        {
          // 3. Если авторизация успешна, запускаем главную форму, 
          // передавая туда контекст и ID вошедшего пользователя
          long userId = authForm.AuthenticatedUserId;
          Application.Run(new Form1(context, userId));
        }
    }
}