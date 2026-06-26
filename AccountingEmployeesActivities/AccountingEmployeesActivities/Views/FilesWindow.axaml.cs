using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;

namespace AccountingEmployeesActivities.Views
{
    public partial class FilesWindow : Window
    {
        public FilesWindow()
        {
            InitializeComponent();

            // После загрузки окна получаем StorageProvider и передаём в ViewModel
            this.Loaded += (s, e) =>
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel != null && DataContext is ViewModels.FilesViewModel vm)
                {
                    vm.SetStorageProvider(topLevel.StorageProvider);
                }
            };
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}