using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AccountingEmployeesActivities.Views
{
    public partial class FilesWindow : Window
    {
        public FilesWindow()
        {
            InitializeComponent();
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}