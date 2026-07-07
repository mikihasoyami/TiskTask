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
        ApplicationConfiguration.Initialize();
        using var context = new AppDbContext();
        using var authForm = new AuthForm(context);

        if (authForm.ShowDialog() == DialogResult.OK)
        {
          var user = context.Users.Find(authForm.AuthenticatedUserId);
          bool isAdmin = user?.IsAdmin ?? false;

          Application.Run(new Form1(context, authForm.AuthenticatedUserId, isAdmin));
        }
  }
}