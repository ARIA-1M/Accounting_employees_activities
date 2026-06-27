using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace AccountingEmployeesActivities.ViewModels.Pages
{
    public class HistoryViewModel : ViewModelBase
    {
        public string Title => "История выполненных задач";
        public ObservableCollection<CompletedTask> CompletedTasks { get; }
        public ICommand OpenCommentsCommand { get; }
        public bool IsBoss { get; }
        private readonly User _currentUser;
        public HistoryViewModel(User currentUser, PostgresContext db, bool isBoss = false)
        {
            _currentUser = currentUser;
            IsBoss = isBoss;
            CompletedTasks = new ObservableCollection<CompletedTask>();
            OpenCommentsCommand = new RelayCommand(OpenComments);
            LoadCompletedTasks(db);
        }

        public HistoryViewModel(User currentUser, bool isBoss = false)
        {
            _currentUser = currentUser;
            IsBoss = isBoss;
            CompletedTasks = new ObservableCollection<CompletedTask>();
            OpenCommentsCommand = new RelayCommand(OpenComments);
            using var db = new PostgresContext();
            LoadCompletedTasks(db);
        }

        private void OpenComments(object parameter)
        {
            if (parameter is CompletedTask task)
            {
                var commentWindow = new CommentWindow();
                commentWindow.DataContext = new CommentViewModel(task.IdTask, _currentUser.IdUser);
                commentWindow.Show();
            }
        }

        private void LoadCompletedTasks(PostgresContext db)
        {
            var doneStatusId = 4;
            var employee = db.Employees.FirstOrDefault(e => e.IdUser == _currentUser.IdUser);
            if (employee == null) return;

            IQueryable<Task> query = db.Tasks
                .Where(t => t.IdStatus == doneStatusId && t.CompletionDate.HasValue)
                .Include(t => t.Files)
                .Include(t => t.Comments)
                .Include(t => t.Executors)
                    .ThenInclude(e => e.IdEmployeeNavigation);

            if (_currentUser.IdRole == 1) // админ – все
            { }
            else if (IsBoss) // руководитель – свои созданные
                query = query.Where(t => t.IdCreator == employee.IdEmployee);
            else // сотрудник – где он исполнитель
                query = query.Where(t => t.Executors.Any(e => e.IdEmployee == employee.IdEmployee && e.IsActive == true));

            // Загружаем данные из БД без преобразования строк
            var rawData = query
                .OrderByDescending(t => t.CompletionDate)
                .Select(t => new
                {
                    t.IdTask,
                    t.Name,
                    t.Description,
                    t.CreationDate,
                    CompletionDate = t.CompletionDate.Value,
                    Files = t.Files.Select(f => f.Name).ToList(),
                    CommentsCount = t.Comments.Count,
                    Executors = t.Executors
                        .Where(e => e.IsActive == true)
                        .Select(e => $"{e.IdEmployeeNavigation.LastName} {e.IdEmployeeNavigation.FirstName}")
                        .ToList()
                })
                .ToList();

            // Преобразуем строки в памяти
            foreach (var item in rawData)
            {
                CompletedTasks.Add(new CompletedTask
                {
                    IdTask = item.IdTask,
                    Name = ToSentenceCase(item.Name),
                    Description = ToSentenceCase(item.Description ?? string.Empty),
                    CreationDate = item.CreationDate,
                    CompletionDate = item.CompletionDate,
                    Status = "Решена",
                    Files = item.Files.Select(f => ToTitleCase(f)).ToList(),
                    CommentsCount = item.CommentsCount,
                    Executors = string.Join(", ", item.Executors.Select(e => ToTitleCase(e)))
                });
            }
        }

        private static string ToTitleCase(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        private static string ToSentenceCase(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            return char.ToUpper(str[0]) + str.Substring(1).ToLower();
        }
    }

    public class CompletedTask
    {
        public int IdTask { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateOnly CreationDate { get; set; }
        public DateOnly CompletionDate { get; set; }
        public string Status { get; set; }
        public List<string> Files { get; set; } = new();
        public int CommentsCount { get; set; }
        public string Executors { get; set; } = "Не указан";

    }
}