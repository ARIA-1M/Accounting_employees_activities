using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AccountingEmployeesActivities.Views.Pages
{
    public partial class TasksView : UserControl
    {
        public TasksView()
        {
            InitializeComponent();
            DataContext = new ViewModels.Pages.TasksViewModel();
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}