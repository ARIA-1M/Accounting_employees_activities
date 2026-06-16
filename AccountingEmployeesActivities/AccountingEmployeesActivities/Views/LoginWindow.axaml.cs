using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.ViewModels;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Text;

// Связь между Views и ViewModels для авторизации

namespace AccountingEmployeesActivities.Views
{
    public partial class LoginWindow : Window
    {
        private LoginViewModel _viewModel;
        public LoginWindow()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;

            _viewModel.LoginSuccess += OnLoginSuccess;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnLoginSuccess(object sender, User user)
        {
            // Открываем главное окно и передаём пользователя
            var mainWindow = new MainWindow(user);
            mainWindow.Show();

            // Закрываем окно входа
            Close();
        }
    }
}
