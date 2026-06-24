using AccountingEmployeesActivities.Models;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AccountingEmployeesActivities.Views.Pages
{
    public partial class DelegationView : UserControl
    {
        public DelegationView()
        {
            InitializeComponent();
        }
        public DelegationView(User currentUser)
        {
            InitializeComponent();
            DataContext = new ViewModels.Pages.DelegatedViewModel(currentUser);
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}