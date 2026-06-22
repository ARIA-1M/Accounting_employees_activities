using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.ViewModels.Dialogs;

namespace AccountingEmployeesActivities.Views.Dialogs
{
    public partial class ChangeStatusDialog : Window
    {
        private ChangeStatusDialogViewModel _viewModel;

        public ChangeStatusDialog()
        {
            InitializeComponent();
        }

        public ChangeStatusDialog(int taskId, int currentStatusId)
        {
            InitializeComponent();

            _viewModel = new ChangeStatusDialogViewModel(taskId, currentStatusId);
            _viewModel.StatusChanged += OnStatusChanged;
            DataContext = _viewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnStatusChanged(object sender, Status status)
        {
            Close();
        }
    }
}

