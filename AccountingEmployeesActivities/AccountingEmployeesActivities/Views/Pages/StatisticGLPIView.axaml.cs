using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.Services;
using AccountingEmployeesActivities.Services.Interfaces;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using System;

namespace AccountingEmployeesActivities.Views.Pages
{
    public partial class StatisticsGLPIView : UserControl
    {
        public StatisticsGLPIView()
        {
            InitializeComponent();
        }
        public StatisticsGLPIView(User currentUser, IStatisticsService statisticsService, IExportService exportService)
        {
            InitializeComponent();
            DataContext = new ViewModels.Pages.StatisticsViewModel(currentUser, statisticsService, exportService);
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
        private async void OnExportClick(object? sender, RoutedEventArgs e)
        {
            // 1. Определяем формат по Tag кнопки
            var button = sender as Button;
            string format = button?.Tag?.ToString() ?? "xlsx";

            // 2. Получаем ViewModel
            var viewModel = DataContext as ViewModels.Pages.StatisticsViewModel;
            if (viewModel is null) return;

            // 3. Получаем окно
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is not Window parentWindow) return;

            // 4. Системный проводник — только ОДИН диалог
            var file = await parentWindow.StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    Title = "Сохранить отчёт",
                    SuggestedFileName = $"Статистика_{DateTime.Now:yyyyMMdd_HHmm}.{format}",
                    DefaultExtension = format,
                    ShowOverwritePrompt = true,
                    FileTypeChoices = new[]
                    {
                    new FilePickerFileType(format == "pdf"
                        ? "PDF файлы"
                        : "Excel файлы")
                    {
                        Patterns = new[] { $"*.{format}" }
                    }
                    }
                });

            if (file is null) return;

            string path = file.TryGetLocalPath();
            if (string.IsNullOrEmpty(path)) return;

            // 5. Вызываем нужный метод ViewModel
            if (format == "pdf")
                await viewModel.ExportPdfAsync(path);
            else
                await viewModel.ExportExcelAsync(path);
        }
    }
}