using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AccountingEmployeesActivities.Views.Pages
{
    public partial class StatisticsView : UserControl
    {
        public StatisticsView()
        {
            InitializeComponent();
            DataContext = new ViewModels.Pages.StatisticsViewModel();
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}