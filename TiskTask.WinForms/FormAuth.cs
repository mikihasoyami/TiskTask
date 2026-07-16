using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TiskTask.Core;
using TiskTask.Services;

namespace TiskTask.WinForms;

public partial class AuthForm : Form
{
  private readonly AppDbContext _context;
  private readonly UserTaskManager _manager;

  public long AuthenticatedUserId { get; private set; }

  private TextBox loginTextBox = null!;
  private TextBox passwordTextBox = null!;
  private Button loginButton = null!;
  private Button registerButton = null!;

  public AuthForm(AppDbContext context)
  {
    _context = context;
    _manager = new UserTaskManager(_context);
    InitializeComponentCustom();
  }

  private void InitializeComponentCustom()
  {
    this.Text = "TiskTask — Авторизация";
    this.FormBorderStyle = FormBorderStyle.FixedDialog;
    this.StartPosition = FormStartPosition.CenterScreen;
    this.ClientSize = new Size(340, 200);
    this.MaximizeBox = false;
    this.MinimizeBox = false;

    var loginLabel = new Label
    {
      Text = "Логин (Имя пользователя):",
      Location = new Point(20, 20),
      Size = new Size(300, 20)
    };

    loginTextBox = new TextBox
    {
      Location = new Point(20, 40),
      Size = new Size(300, 25)
    };

    var passwordLabel = new Label
    {
      Text = "Пароль:",
      Location = new Point(20, 75),
      Size = new Size(300, 20)
    };

    passwordTextBox = new TextBox
    {
      Location = new Point(20, 95),
      Size = new Size(300, 25),
      UseSystemPasswordChar = true
    };

    loginButton = new Button
    {
      Text = "Войти",
      Location = new Point(20, 145),
      Size = new Size(140, 30),
      BackColor = Color.FromArgb(225, 248, 232),
      ForeColor = Color.FromArgb(24, 119, 72)
    };
    loginButton.Click += LoginButton_Click;

    registerButton = new Button
    {
      Text = "Создать аккаунт",
      Location = new Point(180, 145),
      Size = new Size(140, 30)
    };
    registerButton.Click += RegisterButton_Click;

    this.Controls.AddRange(new Control[] {
            loginLabel, loginTextBox,
            passwordLabel, passwordTextBox,
            loginButton, registerButton
        });

    this.AcceptButton = loginButton;
  }

  private void LoginButton_Click(object? sender, EventArgs e)
  {
    var username = loginTextBox.Text.Trim();
    var password = passwordTextBox.Text;

    if (string.IsNullOrWhiteSpace(username))
    {
      MessageBox.Show("Введите имя пользователя.", "Авторизация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      return;
    }

    var user = _context.Users.FirstOrDefault(u => u.Name == username);

    if (user == null || !PasswordHasher.VerifyPassword(password, user.Password))
    {
      MessageBox.Show("Неверное имя пользователя или пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
      return;
    }

    AuthenticatedUserId = user.Id;
    this.DialogResult = DialogResult.OK;
    this.Close();
  }

  private void RegisterButton_Click(object? sender, EventArgs e)
  {
    var username = loginTextBox.Text.Trim();
    var password = passwordTextBox.Text ?? string.Empty;

    if (string.IsNullOrWhiteSpace(username))
    {
      MessageBox.Show("Имя пользователя не может быть пустым.", "Регистрация",
          MessageBoxButtons.OK, MessageBoxIcon.Warning);
      return;
    }

    if (_context.Users.Any(u => u.Name == username))
    {
      MessageBox.Show("Пользователь с таким именем уже существует.", "Регистрация",
          MessageBoxButtons.OK, MessageBoxIcon.Warning);
      return;
    }

    try
    {
      var newUser = new Model.User
      {
        Name = username,
        Password = TiskTask.Services.PasswordHasher.HashPassword(password),
        CreatedAtUtc = DateTime.UtcNow
      };

      _context.Users.Add(newUser);
      _context.SaveChanges();

      MessageBox.Show($"Пользователь {username} успешно создан! Теперь вы можете войти.", "Успех",
          MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    catch (Exception ex)
    {
      MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка",
          MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
  }
}
