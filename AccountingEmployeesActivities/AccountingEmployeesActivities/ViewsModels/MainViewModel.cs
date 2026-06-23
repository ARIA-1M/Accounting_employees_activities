using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.Services;
using AccountingEmployeesActivities.Services.Interfaces;
using AccountingEmployeesActivities.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace AccountingEmployeesActivities.ViewModels
{
    public enum PageType
    {
        MyTasks,
        History,
        Delegation,
        Statistics,
        Tasks,
        Employees,
        Settings   // новый пункт
    }

    public class MainViewModel : ViewModelBase
    {
        private readonly User _currentUser;
        private ViewModelBase _currentPage;
        private Employee _currentEmployee;

        public ObservableCollection<MenuItem> MenuItems { get; }
        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }

        public ViewModelBase CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        // Для верхней панели
        public string FullNameWithRole { get; private set; } = string.Empty;
        public string FirstNameOnly { get; private set; } = string.Empty;

        public MainViewModel(User user)
        {
            _currentUser = user;
            MenuItems = new ObservableCollection<MenuItem>();
            NavigateCommand = new RelayCommand(Navigate);
            LogoutCommand = new RelayCommand(Logout);

            LoadEmployeeData();
            BuildMenu();
            if (MenuItems.Count > 0)
                Navigate(MenuItems[0].PageType);
        }

        private void LoadEmployeeData()
        {
            using var db = new PostgresContext();
            _currentEmployee = db.Employees.FirstOrDefault(e => e.IdUser == _currentUser.IdUser);
            if (_currentEmployee != null)
            {
                var fullName = $"{_currentEmployee.LastName} {_currentEmployee.FirstName}";
                if (!string.IsNullOrEmpty(_currentEmployee.MiddleName))
                    fullName += $" {_currentEmployee.MiddleName}";

                var role = db.Roles.FirstOrDefault(r => r.IdRole == _currentUser.IdRole);
                var roleName = role?.Name ?? "Сотрудник";
                FullNameWithRole = $"{ToTitleCase(fullName)} · {ToTitleCase(roleName)}"; // ФИО и роль – каждое слово с большой
                FirstNameOnly = ToTitleCase(_currentEmployee.FirstName);
            }
            else
            {
                FullNameWithRole = ToTitleCase(_currentUser.Login);
                FirstNameOnly = ToTitleCase(_currentUser.Login);
            }
        }

        private string ToTitleCase(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        private void BuildMenu()
        {
            bool isBoss = _currentUser.IdRole == 1 || _currentUser.IdRole == 2; // администратор или руководитель

            if (isBoss)
            {
                MenuItems.Add(new MenuItem { Header = "ЗАДАЧИ", PageType = PageType.Tasks });
                MenuItems.Add(new MenuItem { Header = "История задач", PageType = PageType.History });
                MenuItems.Add(new MenuItem { Header = "Статистика", PageType = PageType.Statistics });
                MenuItems.Add(new MenuItem { Header = "Сотрудники", PageType = PageType.Employees });
            }
            else
            {
                MenuItems.Add(new MenuItem { Header = "МОИ ЗАДАЧИ", PageType = PageType.MyTasks });
                MenuItems.Add(new MenuItem { Header = "История задач", PageType = PageType.History });
                MenuItems.Add(new MenuItem { Header = "Делегирование", PageType = PageType.Delegation });
                MenuItems.Add(new MenuItem { Header = "Статистика", PageType = PageType.Statistics });
            }
            MenuItems.Add(new MenuItem { Header = "Настройки", PageType = PageType.Settings });
        }

        private void Navigate(object parameter)
        {
            if (parameter is PageType pageType)
            {
                CurrentPage = CreatePage(pageType);
            }
        }

        private ViewModelBase CreatePage(PageType pageType)
        {
            return pageType switch
            {
                PageType.MyTasks => new MyTasksViewModel(),
                PageType.History => new HistoryViewModel(_currentUser, _currentUser.IdRole == 1 || _currentUser.IdRole == 2),
                PageType.Delegation => new DelegationViewModel(),
                PageType.Statistics => new StatisticsViewModel(_currentUser, App.ServiceProvider.GetRequiredService<IStatisticsService>(), App.ServiceProvider.GetRequiredService<IExportService>()),
                PageType.Tasks => new TasksViewModel(),
                PageType.Employees => new EmployeesViewModel(_currentUser),
                PageType.Settings => new SettingsViewModel(),
                _ => new MyTasksViewModel()
            };
        }

        private void Logout(object parameter)
        {
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler LogoutRequested;

        protected void SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }
    }

    public class MenuItem
    {
        public string Header { get; set; }
        public PageType PageType { get; set; }
    }
}