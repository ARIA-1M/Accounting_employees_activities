using AccountingEmployeesActivities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AccountingEmployeesActivities.Views;
using AccountingEmployeesActivities.ViewModels;
using AccountingEmployeesActivities.DTOs;


namespace AccountingEmployeesActivities.ViewModels.Pages
{
    public class MyTasksViewModel : ViewModelBase
    {

        public string Title => "Задачи";
        private string _selectedStatus = "Все статусы";
        private string _dateFromText;
        private string _dateToText;
        private ObservableCollection<TaskCardModel> _tasks = new ObservableCollection<TaskCardModel>();
        private readonly int _currentUserId;
        private readonly int _currentEmployeeId;
        private ObservableCollection<string> _statuses;

        public string SelectedStatus
        {
            get => _selectedStatus;
            set => SetProperty(ref _selectedStatus, value);
        }

        public string DateFromText
        {
            get => _dateFromText;
            set => SetProperty(ref _dateFromText, value);
        }

        public string DateToText
        {
            get => _dateToText;
            set => SetProperty(ref _dateToText, value);
        }

        public ObservableCollection<TaskCardModel> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        public ObservableCollection<string> Statuses
        {
            get => _statuses;
            set => SetProperty(ref _statuses, value);
        }

        public string TasksCount => $"Всего: {Tasks?.Count ?? 0} задач";

        public ICommand ApplyFiltersCommand { get; }
        public ICommand CreateTaskCommand { get; }
        public ICommand ChangeStatusCommand { get; }
        public ICommand DelegateTaskCommand { get; }
        public ICommand OpenFilesCommand { get; }
        public ICommand OpenCommentsCommand { get; }

        public MyTasksViewModel(User currentUser)
        {
            _currentUserId = currentUser.IdUser;

            using var db = new PostgresContext();
            var employee = db.Employees.FirstOrDefault(e => e.IdUser == _currentUserId);
            _currentEmployeeId = employee?.IdEmployee ?? 0;

            Statuses = new ObservableCollection<string>();
            Statuses.Add("Все статусы");
            var statusList = db.Statuses
                .Where(s => s.IdStatus != 5)
                .OrderBy(s => s.IdStatus)
                .Select(s => s.Name)
                .ToList();
            foreach (var status in statusList)
            {
                Statuses.Add(status);
            }

            ApplyFiltersCommand = new RelayCommand(_ => ApplyFilters());
            CreateTaskCommand = new RelayCommand(_ => CreateTask());
            ChangeStatusCommand = new RelayCommand(parameter =>
            {
                if (parameter is TaskCardModel task) ChangeStatus(task);
            });
            DelegateTaskCommand = new RelayCommand(parameter =>
            {
                if (parameter is TaskCardModel task) OpenDelegateDialog(task);
            });
            OpenFilesCommand = new RelayCommand(parameter =>
            {
                if (parameter is TaskCardModel task) OpenFiles(task);
            });
            OpenCommentsCommand = new RelayCommand(parameter =>
            {
                if (parameter is TaskCardModel task) OpenComments(task);
            });

            ApplyFilters();
        }

        public void RefreshData()
        {
            ApplyFilters();
        }

        private DateTime? ParseDate(string dateText)
        {
            if (string.IsNullOrWhiteSpace(dateText))
                return null;

            if (DateTime.TryParseExact(dateText, "dd.MM.yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var date))
            {
                return date;
            }

            return null;
        }

        private void ApplyFilters()
        {
            using var db = new PostgresContext();

            var dateFrom = ParseDate(DateFromText);
            var dateTo = ParseDate(DateToText);

            // Запрос на задачи, где исполнитель сотрудник 
            var query = from task in db.Tasks
                        join executor in db.Executors on task.IdTask equals executor.IdTask
                        where executor.IdEmployee == _currentEmployeeId
                              && executor.IsActive == true
                              && task.IdStatus != 4
                              && task.IdStatus != 5
                        select task;

            // Фильтр по статусу
            if (!string.IsNullOrEmpty(SelectedStatus) && SelectedStatus != "Все статусы")
            {
                var status = db.Statuses.FirstOrDefault(s => s.Name == SelectedStatus);
                if (status != null)
                {
                    query = query.Where(t => t.IdStatus == status.IdStatus);
                }
            }

            // Фильтр по дате "С"
            if (dateFrom.HasValue)
            {
                var dateFromOnly = DateOnly.FromDateTime(dateFrom.Value);
                query = query.Where(t => t.CreationDate >= dateFromOnly);
            }

            // Фильтр по дате "По"
            if (dateTo.HasValue)
            {
                var dateToOnly = DateOnly.FromDateTime(dateTo.Value);
                query = query.Where(t => t.CreationDate <= dateToOnly);
            }

            // Загружаем данные
            var taskList = query
                .Include(t => t.IdStatusNavigation)
                .Include(t => t.IdCreatorNavigation)
                    .ThenInclude(e => e.IdUserNavigation)
                .OrderByDescending(t => t.CreationDate)
                .ToList();

            var cards = new ObservableCollection<TaskCardModel>();
            foreach (var task in taskList)
            {
                // Комментарии
                var comments = db.Comments
                    .Where(c => c.IdTask == task.IdTask)
                    .Include(c => c.IdUserNavigation)        
                        .ThenInclude(u => u.Employee)        
                    .OrderByDescending(c => c.AddDate)
                    .ToList();

                var lastComment = comments.FirstOrDefault();

                // Файлы
                var files = db.Files
                    .Where(f => f.IdTask == task.IdTask)
                    .Select(f => f.Name)
                    .ToList();

                
                // Ищем запись в executor с is_active = false (делегированная задача)
                var delegatedExecutor = db.Executors
                    .FirstOrDefault(e => e.IdTask == task.IdTask && e.IsActive == false);

                // Проверка на возможность делегирования
                bool canDelegate = !(task.IdCreator == _currentEmployeeId) && delegatedExecutor == null;

                System.Diagnostics.Debug.WriteLine($"=== Задача: {task.Name} ===");
                System.Diagnostics.Debug.WriteLine($"IdCreator: {task.IdCreator}, CurrentEmployeeId: {_currentEmployeeId}");
                System.Diagnostics.Debug.WriteLine($"IsCreator: {task.IdCreator == _currentEmployeeId}");
                System.Diagnostics.Debug.WriteLine($"DelegatedExecutor: {(delegatedExecutor != null ? "есть" : "нет")}");
                System.Diagnostics.Debug.WriteLine($"CanDelegate: {canDelegate}");

                var card = new TaskCardModel
                {
                    IdTask = task.IdTask,
                    Title = task.Name,
                    Description = task.Description,
                    StatusText = task.IdStatusNavigation?.Name ?? "Новая",
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
                    CanDelegate = canDelegate,
                    IsDelegated = delegatedExecutor != null,
                    DelegatedTo = delegatedExecutor != null
                        ? $"{delegatedExecutor.IdEmployeeNavigation?.LastName} {delegatedExecutor.IdEmployeeNavigation?.FirstName}".Trim()
                        : null,
                    DelegatedReason = delegatedExecutor?.Comment ?? null
                };

                cards.Add(card);
            }

            Tasks = cards;
        }

        private string GetStatusColor(int statusId)
        {
            return statusId switch
            {
                1 => "#22C55E", // Новая (серый)
                2 => "#FBBF24", // Ожидание (желтый)
                3 => "#3B82F6", // В работе (синий)
                5 => "#FF6F00", // Делегирование (оранжевый)
                _ => "#475569"  // По умолчанию серый
            };
        }

        // Открыть модальное окно создания задачи
        private void CreateTask()
        {

            using var db = new PostgresContext();
            var user = db.Users.FirstOrDefault(u => u.IdUser == _currentUserId);
            bool isBoss = user?.IdRole == 2; 

            var dialog = new AccountingEmployeesActivities.Views.Dialogs.CreateTaskDialog(
                _currentUserId,
                _currentEmployeeId,
                isBoss
            );
            dialog.Show();
        }
        

        //  Открыть окно изменения статуса
        private void ChangeStatus(TaskCardModel task)
        {
            if (task == null) return;

            // Получаем ID статуса из модели
            int currentStatusId = GetStatusIdByName(task.StatusText);

            var dialog = new AccountingEmployeesActivities.Views.Dialogs.ChangeStatusDialog(
                task.IdTask,
                currentStatusId
            );
            dialog.Show();
        }

        private int GetStatusIdByName(string statusName)
        {
            return statusName switch
            {
                "Новая" => 1,
                "В работе" => 2,
                "Ожидание" => 3,
                "Решена" => 4,
                "Делегирование" => 5,
                _ => 1
            };
        }

        // Открыть окно делегирования
        private void DelegateTask(TaskCardModel task)
        {
            if (task == null) return;
            // TODO: Открыть окно делегирования
        }

        // Открыть окно с файлами
        private void OpenFiles(TaskCardModel task)
        {
            if (task == null) return;
            var filesWindow = new FilesWindow();
            filesWindow.DataContext = new FilesViewModel(task.IdTask);
            filesWindow.Show();
        }

        // Окно с коментариями
        private void OpenComments(TaskCardModel task)
        {
            if (task == null) return;
            var commentWindow = new AccountingEmployeesActivities.Views.CommentWindow();
            commentWindow.DataContext = new AccountingEmployeesActivities.ViewModels.CommentViewModel(task.IdTask, _currentUserId);
            commentWindow.Show();
        }
        private void OpenDelegateDialog(TaskCardModel task)
        {
            if (task == null) return;

            var dialog = new AccountingEmployeesActivities.Views.Dialogs.DelegateTaskDialog(task.IdTask, _currentEmployeeId);
            //dialog.Closed += (_, _) => RefreshData();
            dialog.Show();
        }
    }
}