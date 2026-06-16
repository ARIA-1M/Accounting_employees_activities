using AccountingEmployeesActivities.ViewModels;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AccountingEmployeesActivities.Views
{
    public partial class LoginWindow : Window
    {
        private LoginViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            _viewModel.LoginSuccess += OnLoginSuccess;
            DataContext = _viewModel;
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        private void OnLoginSuccess(object sender, Models.User user)
        {
            var mainWindow = new MainWindow(user);
            mainWindow.Show();
            Close();
        }
    }
}