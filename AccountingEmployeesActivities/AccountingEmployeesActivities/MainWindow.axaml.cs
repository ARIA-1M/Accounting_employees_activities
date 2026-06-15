using AccountingEmployeesActivities.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Microsoft.EntityFrameworkCore;
using System;

namespace AccountingEmployeesActivities
{
    public partial class MainWindow : Window
    {
        private TextBox _loginTextBox;
        private TextBox _passwordTextBox;
        private Button _loginButton;
        private TextBlock _errorText;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Загружаем XAML
            AvaloniaXamlLoader.Load(this);

            // Находим элементы на форме
            _loginTextBox = this.FindControl<TextBox>("LoginTextBox");
            _passwordTextBox = this.FindControl<TextBox>("PasswordTextBox");
            _loginButton = this.FindControl<Button>("LoginButton");
            _errorText = this.FindControl<TextBlock>("ErrorText");

            // Подписываемся на клик по кнопке
            _loginButton.Click += OnLoginClick;
        }

        private async void OnLoginClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string login = _loginTextBox.Text?.Trim();
            string password = _passwordTextBox.Text?.Trim();

            // Проверяем, что поля не пустые
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                _errorText.Text = "Введите логин и пароль";
                _errorText.IsVisible = true;
                return;
            }

            // Подключаемся к базе данных
            using var db = new PostgresContext();

            // Ищем пользователя
            var user = await db.Users
                .Include(u => u.IdRoleNavigation)  // Загружаем роль пользователя
                .FirstOrDefaultAsync(u => u.Login == login && u.Password == password);

            if (user == null)
            {
                _errorText.Text = "Неверный логин или пароль";
                _errorText.IsVisible = true;
                return;
            }

            // Вход успешен!
            _errorText.IsVisible = false;

            // Меняем заголовок окна
            this.Title = $"Учёт деятельности сотрудников - Добро пожаловать, {login}";

            // Очищаем окно и показываем приветствие
            var oldContent = this.Content;

            var successPanel = new StackPanel
            {
                Margin = new Thickness(30),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Children =
            {
                new TextBlock
                {
                    Text = "Вход выполнен успешно!",
                    FontSize = 24,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                },
                new TextBlock
                {
                    Text = $"Добро пожаловать, {login}",
                    FontSize = 16,
                    Foreground = new SolidColorBrush(Color.Parse("#64748b")),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Margin = new Thickness(0, 16, 0, 0)
                },
                new TextBlock
                {
                    Text = $"Роль: {user.IdRoleNavigation?.Name ?? "Сотрудник"}",
                    FontSize = 14,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Margin = new Thickness(0, 8, 0, 0)
                }
            }
            };

            this.Content = successPanel;
        }
    }
}