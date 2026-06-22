using AccountingEmployeesActivities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace AccountingEmployeesActivities.ViewModels.Dialogs
{
    public class CreateTaskDialogViewModel : ViewModelBase
    {
        private string _taskName;
        private string _taskDescription;
        private Employee _selectedEmployee;
        private ObservableCollection<Employee> _employees;
        private string _errorMessage;
        private bool _isErrorVisible;
        private readonly int _currentEmployeeId;
        private readonly int _currentUserId;
        private readonly bool _isBoss;

        public string TaskName
        {
            get => _taskName;
            set { _taskName = value; OnPropertyChanged(); }
        }

        public string TaskDescription
        {
            get => _taskDescription;
            set { _taskDescription = value; OnPropertyChanged(); }
        }

        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set { _selectedEmployee = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set { _employees = value; OnPropertyChanged(); }
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

        public bool IsEmployee => !_isBoss;
        public bool IsBoss => _isBoss;
        public string StatusText => "НОВАЯ";
        public string StatusColor => "#3B82F6";

        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler TaskCreated;

        public CreateTaskDialogViewModel(int currentUserId, int currentEmployeeId, bool isBoss)
        {
            _currentUserId = currentUserId;
            _currentEmployeeId = currentEmployeeId;
            _isBoss = isBoss;

            CreateCommand = new RelayCommand(_ => CreateTask());
            CancelCommand = new RelayCommand(_ => Cancel());

            LoadEmployees();
        }

        private void LoadEmployees()
        {
            using var db = new PostgresContext();

            if (_isBoss)
            {
                // Руководитель: выбирает из своих подчинённых
                var employeeList = db.Employees
                    .Include(e => e.IdUserNavigation)
                    .Where(e => e.IdBoss == _currentEmployeeId)
                    .ToList();

                Employees = new ObservableCollection<Employee>(employeeList);

                if (Employees.Any())
                {
                    SelectedEmployee = Employees.First();
                }
            }
            else
            {
                // Сотрудник: только он сам
                var currentEmployee = db.Employees
                    .Include(e => e.IdUserNavigation)
                    .FirstOrDefault(e => e.IdEmployee == _currentEmployeeId);

                Employees = new ObservableCollection<Employee>();
                if (currentEmployee != null)
                {
                    Employees.Add(currentEmployee);
                    SelectedEmployee = currentEmployee;
                }
            }
        }

        private void CreateTask()
        {
            if (string.IsNullOrWhiteSpace(TaskName))
            {
                ShowError("Введите название задачи");
                return;
            }

            if (SelectedEmployee == null)
            {
                ShowError("Выберите исполнителя");
                return;
            }

            using var db = new PostgresContext();

            var task = new Models.Task
            {
                IdStatus = 1,
                IdCreator = _currentEmployeeId,
                Name = TaskName,
                Description = TaskDescription ?? "",
                CreationDate = DateOnly.FromDateTime(DateTime.Now),
                CompletionDate = null
            };

            db.Tasks.Add(task);
            db.SaveChanges();

            var executor = new Executor
            {
                IdTask = task.IdTask,
                IdEmployee = SelectedEmployee.IdEmployee,
                IsActive = true,
                Comment = "Назначен исполнителем",
                ChangeDate = DateOnly.FromDateTime(DateTime.Now)
            };
            


            db.Executors.Add(executor);
            db.SaveChanges();

            TaskCreated?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            TaskCreated?.Invoke(this, EventArgs.Empty);
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            IsErrorVisible = true;
        }
    }
}