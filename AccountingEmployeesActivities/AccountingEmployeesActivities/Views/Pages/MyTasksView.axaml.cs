using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.ViewModels.Pages;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AccountingEmployeesActivities.Views.Pages
{
    public partial class MyTasksView : UserControl
    {
        public MyTasksView()
        {
            InitializeComponent();
        }

        public MyTasksView(User currentUser)
        {
            InitializeComponent();
            DataContext = new MyTasksViewModel(currentUser);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

