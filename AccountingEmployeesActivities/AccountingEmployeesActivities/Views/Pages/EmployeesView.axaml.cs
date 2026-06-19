using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AccountingEmployeesActivities.Models;


namespace AccountingEmployeesActivities.Views.Pages
{
    public partial class EmployeesView : UserControl
    {
        public EmployeesView(User currentUser)  
        {
            InitializeComponent();
            DataContext = new ViewModels.Pages.EmployeesViewModel(currentUser);  
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}