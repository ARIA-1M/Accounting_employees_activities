using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.Services.Interfaces;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AccountingEmployeesActivities.Views.Pages
{
    public partial class StatisticsView : UserControl
    {
        public StatisticsView() 
        {
            InitializeComponent();
        }
        public StatisticsView(User currentUser, IStatisticsService statisticsService)
        {
            InitializeComponent();
            DataContext = new ViewModels.Pages.StatisticsViewModel(currentUser, statisticsService);
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}