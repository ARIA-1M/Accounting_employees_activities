using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AccountingEmployeesActivities.ViewModels.Dialogs;

namespace AccountingEmployeesActivities.Views.Dialogs
{
    public partial class CreateTaskDialog : Window
    {
        private CreateTaskDialogViewModel _viewModel;

        public CreateTaskDialog()
        {
            InitializeComponent();
        }

        public CreateTaskDialog(int currentUserId, int currentEmployeeId, bool isBoss)
        {
            InitializeComponent();

            _viewModel = new CreateTaskDialogViewModel(currentUserId, currentEmployeeId, isBoss);
            _viewModel.TaskCreated += OnTaskCreated;
            DataContext = _viewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnTaskCreated(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}