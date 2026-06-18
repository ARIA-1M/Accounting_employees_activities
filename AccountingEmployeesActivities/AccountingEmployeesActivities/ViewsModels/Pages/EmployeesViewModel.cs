using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.Views.Dialogs;
using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using System;
using Avalonia;
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

            // ← RelayCommand ждёт Action<object>, поэтому передаём методы через лямбды
            CreateEmployeeCommand = new RelayCommand(_ => CreateEmployee());
            EditEmployeeCommand = new RelayCommand(param => EditEmployee(param as Employee));
            ApplyFiltersCommand = new RelayCommand(_ => ApplyFilters());

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
            var dialog = new EmployeeDialog(_currentUserId);
            dialog.Show();  
            ApplyFilters();
        }

        private void EditEmployee(Employee employee)
        {
            if (employee == null) return;
            var dialog = new EmployeeDialog(employee, _currentUserId);
            dialog.Show();  
            ApplyFilters();
        }

        private Window GetMainWindow()
        {
            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }
            return null;
        }
    }
}