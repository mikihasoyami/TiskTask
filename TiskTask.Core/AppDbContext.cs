using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using TiskTask.Model;

namespace TiskTask.Core;

public partial class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
  private const string AppFolderName = "TiskTask";
  private const string DatabaseFileName = "tisktask.db";
  private static readonly string[] LegacyDatabaseFileNames =
  {
        DatabaseFileName,
        "libraryApp.db",
        "telegramBotLibrary.db"
    };

  public AppDbContext()
  {
    InitializeDatabase();
  }

  public AppDbContext(DbContextOptions<AppDbContext> options)
      : base(options)
  {
    InitializeDatabase();
  }

  public virtual DbSet<User> Users { get; set; }
  public virtual DbSet<UserTask> UserTasks { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (!optionsBuilder.IsConfigured)
    {
      var dbPath = GetDatabasePath();
      optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<User>(entity =>
    {
      entity.ToTable("Users");
      entity.HasKey(user => user.Id);
      entity.Property(user => user.Name).IsRequired();
      entity.Property(user => user.Password).IsRequired().HasDefaultValue(string.Empty);
    });

    modelBuilder.Entity<UserTask>(entity =>
    {
      entity.ToTable("UserTask");
      entity.HasKey(task => task.Id);
      entity.Property(task => task.Id).ValueGeneratedOnAdd();

      entity.Property(task => task.Status)
          .HasConversion<int>()
          .HasDefaultValue(UserTaskStatus.New)
          .IsRequired();

      entity.HasOne<User>()
          .WithMany(user => user.Tasks)
          .HasForeignKey(task => task.UserId)
          .OnDelete(DeleteBehavior.Cascade);
    });

    OnModelCreatingPartial(modelBuilder);
  }

  private void InitializeDatabase()
  {
    Database.EnsureCreated();
    EnsureUsersSchema();
    EnsureUserTaskSchema();
    SeedDefaultAdmin();
    SeedLegacyUsers();
  }

  private static string GetDatabasePath()
  {
    var appDataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppFolderName);

    Directory.CreateDirectory(appDataDirectory);

    var appDataDatabasePath = Path.Combine(appDataDirectory, DatabaseFileName);
    TryMigrateLegacyDatabase(appDataDatabasePath);

    return appDataDatabasePath;
  }

  private static void TryMigrateLegacyDatabase(string targetDatabasePath)
  {
    if (File.Exists(targetDatabasePath))
    {
      return;
    }

    foreach (var legacyFileName in LegacyDatabaseFileNames.Distinct(StringComparer.OrdinalIgnoreCase))
    {
      var legacyDatabasePath = Path.Combine(AppContext.BaseDirectory, legacyFileName);
      if (!File.Exists(legacyDatabasePath))
      {
        continue;
      }

      File.Copy(legacyDatabasePath, targetDatabasePath, overwrite: false);
      return;
    }
  }

  private void EnsureUsersSchema()
  {
    Database.ExecuteSqlRaw(
        """
            CREATE TABLE IF NOT EXISTS "Users" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
                "Name" TEXT NOT NULL,
                "Password" TEXT NOT NULL DEFAULT '',
                "CreatedAtUtc" TEXT NOT NULL,
                "IsAdmin" INTEGER NOT NULL DEFAULT 0
            );
            """);
  }

  private void EnsureUserTaskSchema()
  {
    Database.ExecuteSqlRaw(
        """
            CREATE TABLE IF NOT EXISTS "UserTask" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_UserTask" PRIMARY KEY AUTOINCREMENT,
                "UserId" INTEGER NOT NULL,
                "Title" TEXT NOT NULL,
                "Description" TEXT NULL,
                "Created" TEXT NOT NULL,
                "TimeSpent" TEXT NOT NULL,
                CONSTRAINT "FK_UserTask_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
            );
            """);

    var connection = Database.GetDbConnection();
    var shouldCloseConnection = connection.State != ConnectionState.Open;

    if (shouldCloseConnection)
    {
      connection.Open();
    }

    try
    {
      var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      using (var command = connection.CreateCommand())
      {
        command.CommandText = "PRAGMA table_info('UserTask');";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
          existingColumns.Add(reader.GetString(reader.GetOrdinal("name")));
        }
      }

      if (!existingColumns.Contains(nameof(UserTask.IsRunning)))
      {
        Database.ExecuteSqlRaw(
            $"ALTER TABLE \"UserTask\" ADD COLUMN \"{nameof(UserTask.IsRunning)}\" INTEGER NOT NULL DEFAULT 0;");
      }

      if (!existingColumns.Contains(nameof(UserTask.StartedAtUtc)))
      {
        Database.ExecuteSqlRaw(
            $"ALTER TABLE \"UserTask\" ADD COLUMN \"{nameof(UserTask.StartedAtUtc)}\" TEXT NULL;");
      }

      if (!existingColumns.Contains(nameof(UserTask.IsCompleted)))
      {
        Database.ExecuteSqlRaw(
            $"ALTER TABLE \"UserTask\" ADD COLUMN \"{nameof(UserTask.IsCompleted)}\" INTEGER NOT NULL DEFAULT 0;");
      }

      if (!existingColumns.Contains(nameof(UserTask.CompletedAtUtc)))
      {
        Database.ExecuteSqlRaw(
            $"ALTER TABLE \"UserTask\" ADD COLUMN \"{nameof(UserTask.CompletedAtUtc)}\" TEXT NULL;");
      }

      if (!existingColumns.Contains(nameof(UserTask.Status)))
      {
        Database.ExecuteSqlRaw(
            $"ALTER TABLE \"UserTask\" ADD COLUMN \"{nameof(UserTask.Status)}\" INTEGER NOT NULL DEFAULT 0;");

        Database.ExecuteSqlRaw(
            $"UPDATE \"UserTask\" SET \"Status\" = 3 WHERE \"{nameof(UserTask.IsCompleted)}\" = 1;");

        Database.ExecuteSqlRaw(
            $"UPDATE \"UserTask\" SET \"Status\" = 1 WHERE \"{nameof(UserTask.IsRunning)}\" = 1 AND \"{nameof(UserTask.IsCompleted)}\" = 0;");
      }

      var userColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      using (var command = connection.CreateCommand())
      {
        command.CommandText = "PRAGMA table_info('Users');";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
          userColumns.Add(reader.GetString(reader.GetOrdinal("name")));
        }
      }

      if (!userColumns.Contains(nameof(User.Password)))
      {
        Database.ExecuteSqlRaw(
            $"ALTER TABLE \"Users\" ADD COLUMN \"{nameof(User.Password)}\" TEXT NOT NULL DEFAULT '';");
      }
    }
    finally
    {
      if (shouldCloseConnection)
      {
        connection.Close();
      }
    }
  }

  private void SeedDefaultAdmin()
  {
    var adminExists = Users.Any(u => u.Name == "admin");

    if (!adminExists)
    {
      var defaultAdmin = new User
      {
        Name = "admin",
        Password = "admin",
        CreatedAtUtc = DateTime.UtcNow,
        IsAdmin = true
      };

      Users.Add(defaultAdmin);
      SaveChanges();
    }
  }

  private void SeedLegacyUsers()
  {
    var legacyUserIds = UserTasks
        .Select(task => task.UserId)
        .Distinct()
        .ToList();

    if (legacyUserIds.Count == 0)
    {
      return;
    }

    var existingUserIds = Users
        .Select(user => user.Id)
        .ToHashSet();

    var now = DateTime.UtcNow;
    var newUsers = legacyUserIds
        .Where(userId => !existingUserIds.Contains(userId))
        .Select(userId => new User
        {
          Id = userId,
          Name = $"Пользователь {userId}",
          Password = "", 
          CreatedAtUtc = now
        })
        .ToList();

    if (newUsers.Count == 0)
    {
      return;
    }

    Users.AddRange(newUsers);
    SaveChanges();
  }

  partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
