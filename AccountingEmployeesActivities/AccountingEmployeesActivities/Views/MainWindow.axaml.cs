using AccountingEmployeesActivities.Models;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccountingEmployeesActivities.Views
{
    public partial class MainWindow : Window
    {
        private User _currentUser;

        public MainWindow(User user)
        {
            _currentUser = user;
            InitializeComponent();

            var welcomeText = this.FindControl<TextBlock>("WelcomeText");
            var roleText = this.FindControl<TextBlock>("RoleText");
            var logoutButton = this.FindControl<Button>("LogoutButton");

            welcomeText.Text = $"Добро пожаловать, {user.Login}!";
            roleText.Text = $"Роль: {user.IdRoleNavigation?.Name ?? "Сотрудник"}";

            logoutButton.Click += OnLogoutClick;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnLogoutClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {

            var loginWindow = new LoginWindow();
            loginWindow.Show();

            this.Close();
        }
    }
}
