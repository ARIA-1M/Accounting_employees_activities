using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.Services;
using AccountingEmployeesActivities.Services.Implementations;
using AccountingEmployeesActivities.Services.Interfaces;
using AccountingEmployeesActivities.ViewModels.Pages;
using AccountingEmployeesActivities.Views;
using AccountingEmployeesActivities.Views.Pages;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AccountingEmployeesActivities
{
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new LoginWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
        private void ConfigureServices(IServiceCollection services)
        {
            //AddDbContext уже регистрирует как Scoped по умолчанию
            services.AddDbContext<PostgresContext>(options =>
            {
                options.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=123");
            });

            // Сервисы
            services.AddScoped<IStatisticsService, StatisticsService>();
            services.AddTransient<StatisticsViewModel>();
            services.AddTransient<StatisticsView>();
            services.AddSingleton<IExportService, ExportService>();
        }
    }
}