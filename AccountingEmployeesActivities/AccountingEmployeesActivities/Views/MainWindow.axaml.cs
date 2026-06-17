using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.ViewModels;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace AccountingEmployeesActivities.Views
{
    public partial class MainWindow : Window
    {

        public MainWindow() : this(null) { }
        public MainWindow(User user)
        {
            InitializeComponent();
            var vm = new MainViewModel(user);
            vm.LogoutRequested += OnLogoutRequested;
            DataContext = vm;
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        private void OnLogoutRequested(object sender, EventArgs e)
        {
            // Открываем окно входа и закрываем текущее
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}