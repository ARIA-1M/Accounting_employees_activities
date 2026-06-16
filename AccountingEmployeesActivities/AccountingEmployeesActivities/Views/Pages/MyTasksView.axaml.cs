using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AccountingEmployeesActivities.Views.Pages
{
    public partial class MyTasksView : UserControl
    {
        public MyTasksView()
        {
            InitializeComponent();
            DataContext = new ViewModels.Pages.MyTasksViewModel();
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}