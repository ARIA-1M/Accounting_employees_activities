using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AccountingEmployeesActivities.Views.Pages
{
    public partial class HistoryView : UserControl
    {
        public HistoryView()
        {
            InitializeComponent();
            DataContext = new ViewModels.Pages.HistoryViewModel();
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}