using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TiskTask.Model;
using TiskTask.Services;

namespace TiskTask.Core;

public class UserManager
{
  private readonly AppDbContext _context;

  public UserManager(AppDbContext context)
  {
    _context = context ?? throw new ArgumentNullException(nameof(context));
  }

  /// <summary>
  /// Проверяет существование пользователя с таким логином и паролем
  /// </summary>
  public bool IsUser(string login, string password)
  {
    var user = _context.Users.FirstOrDefault(u => u.Name == login);
    if (user == null) return false;

    // Проверяем хэш пароля
    return PasswordHasher.VerifyPassword(password, user.Password);
  }

  /// <summary>
  /// Возвращает объект пользователя для авторизации или null, если данные неверны
  /// </summary>
  public User? GetUser(string login, string password)
  {
    var user = _context.Users
        .AsNoTracking()
        .FirstOrDefault(u => u.Name == login);

    if (user == null) return null;

    if (PasswordHasher.VerifyPassword(password, user.Password))
    {
      return user;
    }

    return null;
  }

  /// <summary>
  /// Регистрирует нового пользователя в базе данных
  /// </summary>
  public void CreateNewUser(string login, string password)
  {
    var isLoginTaken = _context.Users.Any(u => u.Name == login);
    if (isLoginTaken)
    {
      throw new ArgumentException("Пользователь с таким логином уже зарегистрирован!");
    }

    var newUser = new User
    {
      Name = login,
      Password = PasswordHasher.HashPassword(password),
      CreatedAtUtc = DateTime.UtcNow
    };

    _context.Users.Add(newUser);
    _context.SaveChanges();
  }
}
