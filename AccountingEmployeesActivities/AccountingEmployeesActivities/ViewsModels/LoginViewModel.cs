using AccountingEmployeesActivities.Models;
using Avalonia.OpenGL;
using Microsoft.EntityFrameworkCore;
using AccountingEmployeesActivities.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using BCrypt;

// Обработка логики для авторизации

namespace AccountingEmployeesActivities.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _login;
        private string _password;
        private string _errorMessage;
        private bool _isErrorVisible;
        private bool _isLoggedIn;
        private User _currentUser;
        private bool _rememberMe;
        private readonly StatisticGLPIService _settingsService;

        public event EventHandler<User> LoginSuccess;

        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }


        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
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

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set { _isLoggedIn = value; OnPropertyChanged(); }
        }

        public User CurrentUser
        {
            get => _currentUser;
            set { _currentUser = value; OnPropertyChanged(); }
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set { _rememberMe = value; OnPropertyChanged(); }
        }

        public AsyncRelayCommand LoginCommand { get; }

        public LoginViewModel()
        {
            _settingsService = new StatisticGLPIService();
            LoginCommand = new AsyncRelayCommand(LoginAsync);
            IsLoggedIn = false;

            LoadSavedCredentials();
        }

        private void LoadSavedCredentials()
        {
            var settings = _settingsService.LoadCredentials();

            if (!string.IsNullOrEmpty(settings.Login))
            {
                Login = settings.Login;
                Password = settings.Password;
                RememberMe = true;
            }
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Введите логин и пароль";
                IsErrorVisible = true;
                return;
            }

            using var db = new PostgresContext();

            var user = await db.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync<User>(u => u.Login == Login);

            if (user == null)
            {
                ErrorMessage = "Неверный логин или пароль";
                IsErrorVisible = true;
                return;
            }


            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(Password, user.Password);

            if (!isPasswordValid)
            {
                ErrorMessage = "Неверный логин или пароль";
                IsErrorVisible = true;
                return;
            }

            if (user.Employee == null)
            {
                ErrorMessage = "Ошибка: профиль сотрудника не найден";
                IsErrorVisible = true;
                return;
            }

            if (user.Employee.IsActive == false)
            {
                ErrorMessage = "Учётная запись заблокирована. Обратитесь к администратору";
                IsErrorVisible = true;
                return;
            }

            if (RememberMe)
            {
                _settingsService.SaveCredentials(Login, Password);
            }


            CurrentUser = user;
            IsErrorVisible = false;
            IsLoggedIn = true;
            LoginSuccess?.Invoke(this, user);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class AsyncRelayCommand : System.Windows.Input.ICommand
    {
        private readonly Func<Task> _execute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object parameter) => !_isExecuting;

        public async void Execute(object parameter)// Пока идет обработка кнопка блокируется
        {
            _isExecuting = true;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            try
            {
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}