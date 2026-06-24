using AccountingEmployeesActivities.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace AccountingEmployeesActivities.ViewModels.Dialogs
{
    public class ChangeStatusDialogViewModel : ViewModelBase
    {
        private readonly int _taskId;
        private Status _selectedStatus;
        private ObservableCollection<Status> _statuses;
        private string _errorMessage;
        private bool _isErrorVisible;
        private DateTime? _completionDate;

        public Status SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged();

                // Если статус = 4 (Решена) → проставляем дату завершения
                if (value != null && value.IdStatus == 4)
                {
                    CompletionDate = DateTime.Now;
                }
                else
                {
                    CompletionDate = null;
                }
            }
        }

        public ObservableCollection<Status> Statuses
        {
            get => _statuses;
            set { _statuses = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool IsErrorVisible
        {
            get => _isErrorVisible;
            set { _isErrorVisible = value; OnPropertyChanged(); }
        }

        public DateTime? CompletionDate
        {
            get => _completionDate;
            set { _completionDate = value; OnPropertyChanged(); }
        }

        public bool IsCompleted => SelectedStatus?.IdStatus == 4;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<Status> StatusChanged;

        public ChangeStatusDialogViewModel(int taskId, int currentStatusId)
        {
            _taskId = taskId;

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());

            LoadStatuses(currentStatusId);
        }

        private void LoadStatuses(int currentStatusId)
        {
            using var db = new PostgresContext();

            // Получаем все статусы (кроме "Создание" - id=1)
            var statusList = db.Statuses
                .Where(s => s.IdStatus != 1 && s.IdStatus != 5) // исключаем "Создание"
                .OrderBy(s => s.IdStatus)
                .ToList();

            Statuses = new ObservableCollection<Status>(statusList);

            // Выбираем текущий статус
            SelectedStatus = Statuses.FirstOrDefault(s => s.IdStatus == currentStatusId);

            // Если статус = 4 (Решена), показываем дату
            if (currentStatusId == 4)
            {
                using var db2 = new PostgresContext();
                var task = db2.Tasks.FirstOrDefault(t => t.IdTask == _taskId);
                if (task != null && task.CompletionDate.HasValue)
                {
                    CompletionDate = task.CompletionDate.Value.ToDateTime(TimeOnly.MinValue);
                }
            }
        }

        private void Save()
        {
            HideError();

            if (SelectedStatus == null)
            {
                ShowError("Выберите статус");
                return;
            }

            using var db = new PostgresContext();

            var task = db.Tasks.FirstOrDefault(t => t.IdTask == _taskId);
            if (task == null)
            {
                ShowError("Задача не найдена");
                return;
            }

            // Обновляем статус
            task.IdStatus = SelectedStatus.IdStatus;

            // Если статус = 4 (Решена) → ставим дату завершения
            if (SelectedStatus.IdStatus == 4)
            {
                task.CompletionDate = DateOnly.FromDateTime(DateTime.Now);
            }
            else
            {
                // Если статус не "Решена" — очищаем дату завершения
                task.CompletionDate = null;
            }

            db.SaveChanges();

            StatusChanged?.Invoke(this, SelectedStatus);
        }

        private void Cancel()
        {
            StatusChanged?.Invoke(this, null);
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            IsErrorVisible = true;
        }

        private void HideError()
        {
            ErrorMessage = string.Empty;
            IsErrorVisible = false;
        }
    }
}