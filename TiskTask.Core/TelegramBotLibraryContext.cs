using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace TiskTask.Core;

public partial class TelegramBotLibraryContext : DbContext
{
    public TelegramBotLibraryContext()
    {
      Database.EnsureCreated();
    }

    public TelegramBotLibraryContext(DbContextOptions<TelegramBotLibraryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<UserTask> UserTasks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "libraryApp.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserTask>(entity =>
        {
            entity.ToTable("UserTask");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
