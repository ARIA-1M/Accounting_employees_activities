using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AccountingEmployeesActivities.Views.Pages
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            DataContext = new ViewModels.Pages.SettingsViewModel();
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}