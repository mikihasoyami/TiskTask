using Microsoft.EntityFrameworkCore;
using TiskTask.Core;
using TiskTask.Model;

namespace TiskTask.WinForms;

public partial class Form1 : Form
{
    private enum TaskStatusFilter
    {
        All,
        InProgress,
        Paused,
        Completed
    }

    private static readonly AppDbContext _context = new AppDbContext();
    private readonly UserTaskManager _manager = new UserTaskManager(_context);
    private int? _selectedTaskId;
    private long? _selectedUserId;
    private bool _isLoadingUsers;
    private bool _isRefreshing;

    public Form1()
    {
        InitializeComponent();
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        ConfigureStatusFilter();
        LoadUsers();
        RefreshTasks();
        ClearEditor();
        refreshTimer.Start();
    }

    private void refreshTimer_Tick(object? sender, EventArgs e)
    {
        RefreshTasks(keepSelection: true);
    }

    private void tasksListView_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if ((_isRefreshing) || (tasksListView.SelectedItems.Count == 0) || (tasksListView.SelectedItems[0].Tag is not int taskId))
            return;

        SelectTask(taskId);
    }

    private void statusFilterComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        RefreshTasks(keepSelection: true);
    }

    private void usersComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_isLoadingUsers)
        {
            return;
        }

        if (usersComboBox.SelectedItem is User user)
        {
            _selectedUserId = user.Id;
        }
        else
        {
            _selectedUserId = null;
        }

        ClearEditor();
        RefreshTasks();
    }

    private void addUserButton_Click(object? sender, EventArgs e)
    {
    var userName = PromptForText("Новый пользователь", "Введите имя пользователя:");
    if (string.IsNullOrWhiteSpace(userName))
    {
      return;
    }
    try
        {
            var user = _manager.CreateUser(userName);
            LoadUsers(user.Id);
            ClearEditor();
            RefreshTasks();
        }
        catch (ArgumentException exception)
        {
            MessageBox.Show(exception.Message, "Ошибка при создании нового пользователя!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void newTaskButton_Click(object? sender, EventArgs e)
    {
        if (!EnsureUserSelected())
        {
            return;
        }

        ClearEditor();
    }

    private void saveTaskButton_Click(object? sender, EventArgs e)
    {
        if (!EnsureUserSelected())
        {
            return;
        }

        var title = titleTextBox.Text.Trim();
        var description = descriptionTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Название задачи не может быть пустым.", "TiskTask", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_selectedTaskId == null)
        {
            var createdTask = _manager.CreateUserTask(_selectedUserId!.Value, title, description);
            _selectedTaskId = createdTask.Id;
        }
        else
        {
            _manager.ChangeUserTask(_selectedTaskId.Value, title, description, _selectedUserId ?? 0);
        }

        RefreshTasks(keepSelection: true);
    }

    private void deleteTaskButton_Click(object? sender, EventArgs e)
    {
        if (!EnsureUserSelected())
        {
            return;
        }

        if (_selectedTaskId == null)
        {
            MessageBox.Show("Сначала выбери задачу, которую нужно удалить.", "TiskTask", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var task = _manager.GetUserTaskById(_selectedTaskId.Value);
        if (task == null)
        {
          MessageBox.Show(
              "Не удалось удалить задачу, так как она не найдена в базе данных.",
              "Ошибка",
              MessageBoxButtons.OK,
              MessageBoxIcon.Error);
          return;
        }
        var result = MessageBox.Show(
            $"Удалить задачу \"{task.Title}\"?",
            "Подтверждение удаления",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
        {
            return;
        }

        _manager.DeleteUserTask(task.Id);
        ClearEditor();
        RefreshTasks();
    }

    private void completeTaskButton_Click(object? sender, EventArgs e)
    {
        if (!EnsureUserSelected())
        {
            return;
        }

        if (_selectedTaskId == null)
        {
            MessageBox.Show("Сначала выбери задачу, которую нужно завершить.", "TiskTask", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var task = _manager.GetUserTaskById(_selectedTaskId.Value);
        if (task == null)
        {
          MessageBox.Show(
              "Не удалось завершить задачу, так как она не найдена в базе данных.",
              "Ошибка",
              MessageBoxButtons.OK,
              MessageBoxIcon.Error);
          return;
        }
        if (task.IsCompleted)
        {
            MessageBox.Show("Эта задача уже завершена.", "TiskTask", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Завершить задачу \"{task.Title}\"?",
            "Подтверждение завершения",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
        {
            return;
        }

        _manager.CompleteTask(_selectedUserId!.Value, task.Id);
        RefreshTasks(keepSelection: true);
    }

    private void startTaskButton_Click(object? sender, EventArgs e)
    {
        if (!EnsureUserSelected())
        {
            return;
        }

        if (_selectedTaskId == null)
        {
            MessageBox.Show("Выбери задачу, которую нужно запустить.", "TiskTask", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var task = _manager.SwitchActiveTask(_selectedUserId!.Value, _selectedTaskId.Value);
            RefreshTasks(keepSelection: true);
            activeTaskValueLabel.Text = $"[{task.Id}] {task.Title} ({_manager.GetTrackedTime(task.Id):hh\\:mm\\:ss})";
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "TiskTask", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void pauseTaskButton_Click(object? sender, EventArgs e)
    {
        if (!EnsureUserSelected())
        {
            return;
        }

        var activeTask = _manager.GetActiveTask(_selectedUserId!.Value);
        if (activeTask == null)
        {
            MessageBox.Show("Сейчас нет активной задачи.", "TiskTask", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _manager.StopActiveTask(_selectedUserId!.Value);
        RefreshTasks(keepSelection: true);
    }

    private void RefreshTasks(bool keepSelection = false)
    {
      _isRefreshing = true;

      var allUsers = _manager.GetAllUsers();
      var userDictionary = allUsers.ToDictionary(user => user.Id, user => user.Name);
      var allTasks = _manager.GetAllTasks();
      var sortedTasks = allTasks.OrderBy(x => x.Id).ToList();
      var filteredTasks = ApplyStatusFilter(sortedTasks).ToList();
      var selectedTaskId = keepSelection ? _selectedTaskId : null;

      tasksListView.BeginUpdate();
      tasksListView.Items.Clear();

      foreach (var task in filteredTasks)
      {
        var userName = userDictionary.ContainsKey(task.UserId)
            ? userDictionary[task.UserId]
            : $"Пользователь {task.UserId}";

        var item = new ListViewItem(userName);
        item.SubItems.Add(task.Title);
        item.SubItems.Add(GetTaskStatusText(task));
        item.SubItems.Add(_manager.GetTrackedTime(task.Id).ToString(@"hh\:mm\:ss"));
        item.Tag = task.Id;
        item.BackColor = GetTaskBackColor(task);
        item.ForeColor = GetTaskForeColor(task);
        tasksListView.Items.Add(item);

        if (selectedTaskId == task.Id)
        {
          item.Selected = true;
        }
      }

      tasksListView.EndUpdate();
      _isRefreshing = false;

      UpdateActiveTaskLabel();

      if (_selectedTaskId != null && !filteredTasks.Any(x => x.Id == _selectedTaskId.Value))
      {
        ClearEditor();
      }
    }


    private void SelectTask(int taskId)
    {
        var task = _manager.GetUserTaskById(taskId);
        if (task == null)
        {
          MessageBox.Show(
              "Не удалось выбрать задачу, так как она не найдена в базе данных.",
              "Ошибка",
              MessageBoxButtons.OK,
              MessageBoxIcon.Error);
          return;
        }
        UpdateEditor(task);
    }

    private void UpdateEditor(UserTask task)
    {
        _selectedTaskId = task.Id;
        titleTextBox.Text = task.Title;
        descriptionTextBox.Text = task.Description;
        createdValueLabel.Text = BuildTaskDateText(task);
    }

    private void ClearEditor()
    {
        _selectedTaskId = null;
        tasksListView.SelectedItems.Clear();
        titleTextBox.Text = string.Empty;
        descriptionTextBox.Text = string.Empty;
        createdValueLabel.Text = _selectedUserId == null ? "Сначала выбери пользователя" : "Новая задача";
    }

    private void UpdateActiveTaskLabel()
    {
        if (_selectedUserId == null)
        {
            activeTaskValueLabel.Text = "Сначала создай или выбери пользователя";
            activeTaskValueLabel.ForeColor = Color.DimGray;
            return;
        }

        var activeTask = _manager.GetActiveTask(_selectedUserId.Value);
        if (activeTask == null)
        {
            activeTaskValueLabel.Text = "Нет активной задачи";
            activeTaskValueLabel.ForeColor = Color.DimGray;
            return;
        }

        activeTaskValueLabel.Text = $"[{activeTask.Id}] {activeTask.Title} ({_manager.GetTrackedTime(activeTask.Id):hh\\:mm\\:ss})";
        activeTaskValueLabel.ForeColor = Color.FromArgb(24, 119, 72);
    }

    private static string GetTaskStatusText(UserTask task)
    {
        if (task.IsCompleted)
        {
            return "Завершена";
        }

        return task.IsRunning ? "В работе" : "На паузе";
    }

    private static string BuildTaskDateText(UserTask task)
    {
        var createdText = task.Created == default
            ? "Новая задача"
            : $"Создана: {task.Created.ToLocalTime():dd.MM.yyyy HH:mm:ss}";

        if (!task.IsCompleted || task.CompletedAtUtc == null)
        {
            return createdText;
        }

        return $"{createdText} | Завершена: {task.CompletedAtUtc.Value.ToLocalTime():dd.MM.yyyy HH:mm:ss}";
    }

    private static Color GetTaskBackColor(UserTask task)
    {
        if (task.IsCompleted)
        {
            return Color.FromArgb(240, 240, 240);
        }

        if (task.IsRunning)
        {
            return Color.FromArgb(225, 248, 232);
        }

        return Color.FromArgb(255, 247, 221);
    }

    private static Color GetTaskForeColor(UserTask task)
    {
        if (task.IsCompleted)
        {
            return Color.FromArgb(90, 90, 90);
        }

        if (task.IsRunning)
        {
            return Color.FromArgb(24, 119, 72);
        }

        return Color.FromArgb(143, 94, 0);
    }

    private void ConfigureStatusFilter()
    {
        statusFilterComboBox.DisplayMember = "Text";
        statusFilterComboBox.ValueMember = "Value";
        statusFilterComboBox.DataSource = new[]
        {
            new { Text = "Все", Value = TaskStatusFilter.All },
            new { Text = "В работе", Value = TaskStatusFilter.InProgress },
            new { Text = "На паузе", Value = TaskStatusFilter.Paused },
            new { Text = "Завершена", Value = TaskStatusFilter.Completed }
        };
    }

    private IEnumerable<UserTask> ApplyStatusFilter(IEnumerable<UserTask> tasks)
    {
        var filter = GetSelectedStatusFilter();

        return filter switch
        {
            TaskStatusFilter.InProgress => tasks.Where(task => task.IsRunning && !task.IsCompleted),
            TaskStatusFilter.Paused => tasks.Where(task => !task.IsRunning && !task.IsCompleted),
            TaskStatusFilter.Completed => tasks.Where(task => task.IsCompleted),
            _ => tasks
        };
    }

    private TaskStatusFilter GetSelectedStatusFilter()
    {
        if (statusFilterComboBox.SelectedItem == null)
        {
            return TaskStatusFilter.All;
        }

        var property = statusFilterComboBox.SelectedItem.GetType().GetProperty("Value");
        if (property?.GetValue(statusFilterComboBox.SelectedItem) is TaskStatusFilter filter)
        {
            return filter;
        }

        return TaskStatusFilter.All;
    }

    private void LoadUsers(long? selectedUserId = null)
    {
        var users = _manager.GetAllUsers();

        _isLoadingUsers = true;
        usersComboBox.DataSource = null;
        if (users.Count == 0)
        {
            _selectedUserId = null;
            usersComboBox.Enabled = false;
            _isLoadingUsers = false;
            return;
        }

        usersComboBox.Enabled = true;
        usersComboBox.DisplayMember = nameof(User.Name);
        usersComboBox.ValueMember = nameof(User.Id);
        usersComboBox.DataSource = users;

        var targetUserId = selectedUserId ?? _selectedUserId ?? users[0].Id;
        var targetUser = users.FirstOrDefault(user => user.Id == targetUserId) ?? users[0];
        usersComboBox.SelectedItem = targetUser;
        _selectedUserId = targetUser.Id;
        _isLoadingUsers = false;
    }

    private bool EnsureUserSelected()
    {
        if (_selectedUserId != null)
        {
            return true;
        }

        MessageBox.Show("Сначала создай или выбери пользователя.", "TiskTask", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return false;
    }

    private string? PromptForText(string title, string prompt)
    {
        using var dialog = new Form();
        using var inputTextBox = new TextBox();
        using var promptLabel = new Label();
        using var okButton = new Button();
        using var cancelButton = new Button();

        dialog.Text = title;
        dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
        dialog.StartPosition = FormStartPosition.CenterParent;
        dialog.ClientSize = new Size(380, 145);
        dialog.MaximizeBox = false;
        dialog.MinimizeBox = false;
        dialog.ShowInTaskbar = false;

        promptLabel.AutoSize = true;
        promptLabel.Text = prompt;
        promptLabel.Location = new Point(12, 15);

        inputTextBox.Location = new Point(12, 45);
        inputTextBox.Size = new Size(350, 23);

        okButton.Text = "Создать";
        okButton.DialogResult = DialogResult.OK;
        okButton.Location = new Point(206, 95);
        okButton.Size = new Size(75, 27);

        cancelButton.Text = "Отмена";
        cancelButton.DialogResult = DialogResult.Cancel;
        cancelButton.Location = new Point(287, 95);
        cancelButton.Size = new Size(75, 27);

        dialog.Controls.Add(promptLabel);
        dialog.Controls.Add(inputTextBox);
        dialog.Controls.Add(okButton);
        dialog.Controls.Add(cancelButton);
        dialog.AcceptButton = okButton;
        dialog.CancelButton = cancelButton;

        return dialog.ShowDialog(this) == DialogResult.OK
            ? inputTextBox.Text.Trim()
            : null;
    }
}
