using AccountingEmployeesActivities.DTOs;
using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.Views;
using global::AccountingEmployeesActivities.Models;
using global::AccountingEmployeesActivities.Views;
using global::AccountingEmployeesActivities.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
namespace AccountingEmployeesActivities.ViewModels.Pages
{

    public class DelegatedViewModel : ViewModelBase
    {
        public string Title => "Делегированные задачи";

        private ObservableCollection<TaskCardModel> _tasks = new ObservableCollection<TaskCardModel>();
        private readonly int _currentUserId;
        private readonly int _currentEmployeeId;

        public ObservableCollection<TaskCardModel> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        public string TasksCount => $"Всего: {Tasks?.Count ?? 0} задач";

        public ICommand OpenFilesCommand { get; }
        public ICommand OpenCommentsCommand { get; }
        public ICommand RefreshCommand { get; }

        public DelegatedViewModel(User currentUser)
        {
            _currentUserId = currentUser.IdUser;

            using var db = new PostgresContext();
            var employee = db.Employees.FirstOrDefault(e => e.IdUser == _currentUserId);
            _currentEmployeeId = employee?.IdEmployee ?? 0;

            OpenFilesCommand = new RelayCommand(parameter =>
            {
                if (parameter is TaskCardModel task) OpenFiles(task);
            });

            OpenCommentsCommand = new RelayCommand(parameter =>
            {
                if (parameter is TaskCardModel task) OpenComments(task);
            });

            RefreshCommand = new RelayCommand(_ => LoadDelegatedTasks());

            LoadDelegatedTasks();
        }

        public void RefreshData()
        {
            LoadDelegatedTasks();
        }

        private void LoadDelegatedTasks()
        {
            using var db = new PostgresContext();

            const int delegationStatusId = 5;

            var taskList = (from task in db.Tasks
                            join executor in db.Executors on task.IdTask equals executor.IdTask
                            where task.IdStatus == delegationStatusId
                                  && executor.IdEmployee == _currentEmployeeId
                                  && executor.IsActive == true
                            select task)
                .Include(t => t.IdStatusNavigation)
                .OrderByDescending(t => t.CreationDate)
                .Distinct()
                .ToList();

            var cards = new ObservableCollection<TaskCardModel>();

            foreach (var task in taskList)
            {
                var comments = db.Comments
                    .Where(c => c.IdTask == task.IdTask)
                    .Include(c => c.IdUserNavigation)
                        .ThenInclude(u => u.Employee)
                    .OrderByDescending(c => c.AddDate)
                    .ToList();

                var lastComment = comments.FirstOrDefault();

                var files = db.Files
                    .Where(f => f.IdTask == task.IdTask)
                    .Select(f => f.Name)
                    .ToList();

                var activeExecutor = db.Executors
                    .Include(e => e.IdEmployeeNavigation)
                    .FirstOrDefault(e => e.IdTask == task.IdTask && e.IsActive == true);

                var card = new TaskCardModel
                {
                    IdTask = task.IdTask,
                    Title = task.Name,
                    Description = task.Description,
                    StatusText = task.IdStatusNavigation?.Name ?? "Делегирование",
                    StatusColor = GetStatusColor(task.IdStatus),
                    CreatedDate = task.CreationDate.ToString("dd.MM.yyyy"),
                    Deadline = task.CompletionDate?.ToString("dd.MM.yyyy") ?? "Не указан",
                    CommentsCount = comments.Count,
                    LastCommentAuthor = lastComment != null
                        ? $"{lastComment.IdUserNavigation?.Employee?.LastName} {lastComment.IdUserNavigation?.Employee?.FirstName}".Trim()
                        : null,
                    LastCommentText = lastComment?.Text?.Length > 60
                        ? lastComment.Text.Substring(0, 60) + "..."
                        : lastComment?.Text,
                    LastCommentDate = lastComment?.AddDate.ToString("dd.MM.yyyy"),
                    Files = files,
                    CanDelegate = false,
                    IsDelegated = true,
                    DelegatedTo = activeExecutor != null
                        ? $"{activeExecutor.IdEmployeeNavigation?.LastName} {activeExecutor.IdEmployeeNavigation?.FirstName}".Trim()
                        : null,
                    DelegatedReason = activeExecutor?.Comment
                };

                cards.Add(card);
            }

            Tasks = cards;
            OnPropertyChanged(nameof(TasksCount));
        }
        private string GetStatusColor(int statusId)
        {
            return statusId switch
            {
                1 => "#22C55E",
                2 => "#3B82F6",
                3 => "#FACC15",
                4 => "#16A34A",
                5 => "#FF6F00",
                _ => "#475569"
            };
        }

        private void OpenFiles(TaskCardModel task)
        {
            if (task == null) return;

            var filesWindow = new FilesWindow();
            filesWindow.DataContext = new FilesViewModel(task.IdTask);
            filesWindow.Show();
        }

        private void OpenComments(TaskCardModel task)
        {
            if (task == null) return;

            var commentWindow = new AccountingEmployeesActivities.Views.CommentWindow();
            commentWindow.DataContext = new AccountingEmployeesActivities.ViewModels.CommentViewModel(task.IdTask, _currentUserId);
            commentWindow.Show();
        }
    }

}