using AccountingEmployeesActivities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace AccountingEmployeesActivities.ViewModels.Pages
{
    public class EmployeesViewModel : ViewModelBase
    {
        private string _searchText = string.Empty;
        private bool _hideFired = true;
        private ObservableCollection<Employee> _employees = new ObservableCollection<Employee>();
        private Employee _selectedEmployee;
        private readonly int _currentUserId;

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public bool HideFired
        {
            get => _hideFired;
            set => SetProperty(ref _hideFired, value);
        }

        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set => SetProperty(ref _employees, value);
        }

        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set => SetProperty(ref _selectedEmployee, value);
        }

        public string EmployeesCount => $"Всего: {Employees?.Count ?? 0} сотрудников";

        public ICommand CreateEmployeeCommand { get; }
        public ICommand EditEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }
        public ICommand ApplyFiltersCommand { get; }

        public EmployeesViewModel(User currentUser)
        {
            _currentUserId = currentUser.IdUser;

            CreateEmployeeCommand = new RelayCommand(CreateEmployee);
            EditEmployeeCommand = new RelayCommand<Employee>(EditEmployee);
            DeleteEmployeeCommand = new RelayCommand<Employee>(DeleteEmployee);
            ApplyFiltersCommand = new RelayCommand(ApplyFilters);

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            using var db = new PostgresContext();

            var query = db.Employees
                .Include(e => e.IdUserNavigation)
                    .ThenInclude(u => u.IdRoleNavigation)
                .Where(e => e.IdBoss == _currentUserId);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.Trim();
                query = query.Where(e =>
                    e.FirstName.Contains(search) ||
                    e.LastName.Contains(search) ||
                    e.MiddleName.Contains(search)
                );
            }

            if (HideFired)
            {
                query = query.Where(e => e.IsActive == true);
            }

            Employees = new ObservableCollection<Employee>(query.ToList());
        }

        private void CreateEmployee()
        {
            // TODO: Открыть модальное окно для создания
        }

        private void EditEmployee(Employee employee)
        {
            if (employee == null) return;
            // TODO: Открыть модальное окно для редактирования
        }

        private void DeleteEmployee(Employee employee)
        {
            if (employee == null) return;
            // TODO: Удалить сотрудника
        }
    }

    // ========== RelayCommand ==========
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
        public void Execute(object parameter) => _execute();
        public event EventHandler CanExecuteChanged;
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);
        public void Execute(object parameter) => _execute((T)parameter);
        public event EventHandler CanExecuteChanged;
    }
}