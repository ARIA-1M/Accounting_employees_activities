using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AccountingEmployeesActivities.Views.Pages
{
    public partial class DelegationView : UserControl
    {
        public DelegationView()
        {
            InitializeComponent();
            DataContext = new ViewModels.Pages.DelegationViewModel();
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}