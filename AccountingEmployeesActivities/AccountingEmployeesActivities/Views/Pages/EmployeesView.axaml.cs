using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AccountingEmployeesActivities.Views.Pages
{
    public partial class EmployeesView : UserControl
    {
        public EmployeesView()
        {
            InitializeComponent();
            DataContext = new ViewModels.Pages.EmployeesViewModel();
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}