using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.ViewModels.Dialogs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace AccountingEmployeesActivities.Views.Dialogs
{
    public partial class EmployeeDialog : Window
    {
        private EmployeeDialogViewModel _viewModel;

        public EmployeeDialog()
        {
            InitializeComponent();
        }

        public EmployeeDialog(int currentUserId)
        {
            InitializeComponent();
            _viewModel = new EmployeeDialogViewModel(currentUserId);
            _viewModel.EmployeeSaved += OnEmployeeSaved;
            DataContext = _viewModel;
        }

        public EmployeeDialog(Employee employee, int currentUserId)
        {
            InitializeComponent();
            _viewModel = new EmployeeDialogViewModel(employee, currentUserId);
            _viewModel.EmployeeSaved += OnEmployeeSaved;
            DataContext = _viewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnEmployeeSaved(object sender, EmployeeFormModel form)
        {
            if (form != null)
            {
                SaveOrUpdateEmployee(form);
            }
            Close();
        }

        private void SaveOrUpdateEmployee(EmployeeFormModel form)
        {
            using var db = new PostgresContext();

            if (form.IdEmployee == null)
            {
                // ===== СОЗДАНИЕ =====
                var user = new User
                {
                    Login = form.Login,
                    Password = BCrypt.Net.BCrypt.HashPassword(form.Password, workFactor: 10),
                    IdRole = form.IdRole
                };
                user.IdUser = db.Users.Any() ? db.Users.Max(u => u.IdUser) + 1 : 1;
                db.Users.Add(user);
                db.SaveChanges();

                

                var employee = new Employee
                {
                    IdUser = user.IdUser,
                    FirstName = form.FirstName,
                    LastName = form.LastName,
                    MiddleName = form.MiddleName,
                    IdBoss = form.IdBoss,
                    IsActive = form.IsActive
                };
                employee.IdEmployee = db.Employees.Any() ? db.Employees.Max(e => e.IdEmployee) + 1 : 1;
                db.Employees.Add(employee);
                db.SaveChanges();
            }
            else
            {
                // ===== РЕДАКТИРОВАНИЕ =====
                var employee = db.Employees
                    .Include(e => e.IdUserNavigation)
                    .FirstOrDefault(e => e.IdEmployee == form.IdEmployee);

                if (employee == null) return;

                employee.FirstName = form.FirstName;
                employee.LastName = form.LastName;
                employee.MiddleName = form.MiddleName;

                employee.IsActive = form.IsActive;

                if (employee.IdUserNavigation != null)
                {
                    employee.IdUserNavigation.Login = form.Login;
                    if (!string.IsNullOrWhiteSpace(form.Password))
                    {
                        employee.IdUserNavigation.Password = BCrypt.Net.BCrypt.HashPassword(form.Password, workFactor: 10);
                    }
                    employee.IdUserNavigation.IdRole = form.IdRole;
                }

                db.SaveChanges();
            }
        }
    }
}