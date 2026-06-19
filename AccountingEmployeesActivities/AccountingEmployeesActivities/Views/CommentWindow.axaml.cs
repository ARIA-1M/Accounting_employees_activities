using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AccountingEmployeesActivities.Views
{
    public partial class CommentWindow : Window
    {
        public CommentWindow()
        {
            InitializeComponent();
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}