using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AccountingEmployeesActivities.Views.Pages
{
    public partial class StatisticGLPIView : UserControl
    {
        public StatisticGLPIView()
        {
            InitializeComponent();
            DataContext = new ViewModels.Pages.StatisticGLPIViewModel();
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}