using AccountingEmployeesActivities.DTOs;
using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.Services;
using AccountingEmployeesActivities.Services.Interfaces;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SystemTask = System.Threading.Tasks.Task;

namespace AccountingEmployeesActivities.ViewModels.Pages
{
    public class StatisticsGLPIViewModel : ViewModelBase
    {
        private readonly IStatisticsService _statisticsService;
        private readonly int _currentUserId;
        private readonly int _currentEmployeeId;

        #region Properties

        private string _title = "Статистика сотрудников";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private readonly IExportService _exportService;
        public ICommand ExportToExcelCommand { get; }
        public ICommand ExportToPdfCommand { get; }

        private ISeries[] _statusChartSeries = Array.Empty<ISeries>();
        public ISeries[] StatusChartSeries
        {
            get => _statusChartSeries;
            set => SetProperty(ref _statusChartSeries, value);
        }

        private ISeries[] _employeeTasksChartSeries = Array.Empty<ISeries>();
        public ISeries[] EmployeeTasksChartSeries
        {
            get => _employeeTasksChartSeries;
            set => SetProperty(ref _employeeTasksChartSeries, value);
        }

        private Axis[] _employeeTasksXAxes = Array.Empty<Axis>();
        public Axis[] EmployeeTasksXAxes
        {
            get => _employeeTasksXAxes;
            set => SetProperty(ref _employeeTasksXAxes, value);
        }

        private Axis[] _employeeTasksYAxes = Array.Empty<Axis>();
        public Axis[] EmployeeTasksYAxes
        {
            get => _employeeTasksYAxes;
            set => SetProperty(ref _employeeTasksYAxes, value);
        }

        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                    _ = LoadStatisticsAsync();
            }
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                    _ = LoadStatisticsAsync();
            }
        }

        private int _totalTasks;
        public int TotalTasks
        {
            get => _totalTasks;
            set => SetProperty(ref _totalTasks, value);
        }

        private int _completedTasks;
        public int CompletedTasks
        {
            get => _completedTasks;
            set => SetProperty(ref _completedTasks, value);
        }

        private int _inProgressTasks;
        public int InProgressTasks
        {
            get => _inProgressTasks;
            set => SetProperty(ref _inProgressTasks, value);
        }

        private int _progressPercentage;
        public int ProgressPercentage
        {
            get => _progressPercentage;
            set => SetProperty(ref _progressPercentage, value);
        }

        private ObservableCollection<EmployeeFilterDto> _employees = new();
        public ObservableCollection<EmployeeFilterDto> Employees
        {
            get => _employees;
            set => SetProperty(ref _employees, value);
        }

        private EmployeeFilterDto _selectedEmployee;
        public EmployeeFilterDto SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                if (SetProperty(ref _selectedEmployee, value))
                    _ = LoadStatisticsAsync();
            }
        }

        private ObservableCollection<StatusDistributionDto> _statusDistribution = new();
        public ObservableCollection<StatusDistributionDto> StatusDistribution
        {
            get => _statusDistribution;
            set => SetProperty(ref _statusDistribution, value);
        }

        private ObservableCollection<EmployeeTasksDto> _employeeTasks = new();
        public ObservableCollection<EmployeeTasksDto> EmployeeTasks
        {
            get => _employeeTasks;
            set => SetProperty(ref _employeeTasks, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        #endregion

        public ICommand RefreshCommand { get; }

        public StatisticsGLPIViewModel(User currentUser, IStatisticsService statisticsService)
        {
            _currentUserId = currentUser.IdUser;

            using var db = new PostgresContext();
            var employee = db.Employees.FirstOrDefault(e => e.IdUser == _currentUserId);
            _currentEmployeeId = employee?.IdEmployee ?? 0;

            _statisticsService = statisticsService;
            RefreshCommand = new RelayCommand(_ =>_ = RefreshAsync());

            // ИНИЦИАЛИЗАЦИЯ ДАТ ПО УМОЛЧАНИЮ (ПОСЛЕДНИЙ МЕСЯЦ)
            _endDate = DateTime.Now;
            _startDate = DateTime.Now.AddMonths(-1);

            _ = InitializeAsync();
        }

        private async SystemTask InitializeAsync()
        {
            IsLoading = true;
            try
            {

                var employees = await _statisticsService.GetEmployeesForFilterAsync(_currentEmployeeId);
                Employees = new ObservableCollection<EmployeeFilterDto>(employees ?? new());
                SelectedEmployee = Employees.FirstOrDefault();

                if (SelectedEmployee != null)
                    await LoadStatisticsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Initialize error: " + ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async SystemTask RefreshAsync()
        {
            IsLoading = true;
            try
            {
                await LoadStatisticsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Refresh error: " + ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async SystemTask LoadStatisticsAsync()
        {
            System.Diagnostics.Debug.WriteLine($"LoadStatisticsAsync START");

            if (SelectedEmployee == null)
            {
                System.Diagnostics.Debug.WriteLine($"   SelectedEmployee is NULL!");
                return;
            }

            IsLoading = true;

            try
            {
                int? filterId = SelectedEmployee.Id == 0 ? null : SelectedEmployee.Id;
                System.Diagnostics.Debug.WriteLine($"Фильтр по сотруднику: {filterId?.ToString() ?? "Все"}");

                var stats = await _statisticsService.GetStatisticsAsync(filterId, _startDate, _endDate);
                System.Diagnostics.Debug.WriteLine($"Stats получены: Total={stats.TotalTasks}, Completed={stats.CompletedTasks}");

                TotalTasks = stats.TotalTasks;
                CompletedTasks = stats.CompletedTasks;
                InProgressTasks = stats.InProgressTasks;
                ProgressPercentage = stats.ProgressPercentage;

                var distribution = await _statisticsService.GetStatusDistributionAsync(filterId, _startDate, _endDate);
                System.Diagnostics.Debug.WriteLine($"Distribution получена: {distribution?.Count ?? 0} записей");

                StatusDistribution = new ObservableCollection<StatusDistributionDto>(distribution ?? new());

                var employeeTasks = await _statisticsService.GetEmployeeTasksAsync(filterId, _startDate, _endDate);
                System.Diagnostics.Debug.WriteLine($"EmployeeTasks получены: {employeeTasks?.Count ?? 0} записей");

                EmployeeTasks = new ObservableCollection<EmployeeTasksDto>(employeeTasks ?? new());

                BuildCharts();
                System.Diagnostics.Debug.WriteLine($"Диаграммы построены");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ОШИБКА: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Стек: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine($"LoadStatisticsAsync END");
            }
        }

        private void BuildCharts()
        {
            BuildStatusChart();
            BuildEmployeeTasksChart();
        }

        private void BuildStatusChart()
        {
            if (StatusDistribution == null || StatusDistribution.Count == 0)
            {
                StatusChartSeries = Array.Empty<ISeries>();
                return;
            }

            StatusChartSeries = StatusDistribution
                .Select((item, index) => new PieSeries<double>
                {
                    Name = item.StatusName,
                    Values = new[] { (double)item.Count },
                    Fill = new SolidColorPaint(GetStatusColor(item.StatusName)),
                    Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                    DataLabelsSize = 20,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White)
                    {
                        SKTypeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold)
                    }
                })
                .ToArray();
        }

        private void BuildEmployeeTasksChart()
        {
            if (EmployeeTasks == null || EmployeeTasks.Count == 0)
            {
                EmployeeTasksChartSeries = Array.Empty<ISeries>();
                EmployeeTasksXAxes = Array.Empty<Axis>();
                EmployeeTasksYAxes = Array.Empty<Axis>();
                return;
            }

            var labels = EmployeeTasks
                .Select(x => x.EmployeeName)
                .ToArray();

            var completedValues = EmployeeTasks
                .Select(x => (double)x.CompletedTasks)
                .ToArray();

            var remainingValues = EmployeeTasks
                .Select(x => (double)Math.Max(0, x.TotalTasks - x.CompletedTasks))
                .ToArray();

            EmployeeTasksChartSeries =
            [
                new StackedColumnSeries<double>
                {
                    Name = "Выполнено",
                    Values = completedValues,
                    Fill = new SolidColorPaint(SKColor.Parse("#23c55e")),
                    Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 1 },
                    DataLabelsSize = 20,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White)
                    {
                        SKTypeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold)
                    },
                    DataLabelsFormatter = point =>
                        point.Coordinate.PrimaryValue == 0 ? string.Empty : point.Coordinate.PrimaryValue.ToString("0"),
                    MaxBarWidth = 55
                },
                new StackedColumnSeries<double>
                {
                    Name = "Осталось",
                    Values = remainingValues,
                    Fill = new SolidColorPaint(SKColor.Parse("#BDBDBD")),
                    Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 1 },
                    DataLabelsSize = 20,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White)
                    {
                        SKTypeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold)
                    },
                    DataLabelsFormatter = point =>
                        point.Coordinate.PrimaryValue == 0 ? string.Empty : point.Coordinate.PrimaryValue.ToString("0"),
                    MaxBarWidth = 55
                }
            ];

            EmployeeTasksXAxes =
            [
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 15,
                    TextSize = 12,
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#333333")),
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#EEEEEE"))
                }
            ];

            EmployeeTasksYAxes =
            [
                new Axis
                {
                    MinLimit = 0,
                    TextSize = 12,
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#333333")),
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#DDDDDD")),
                    Labeler = value => value.ToString("0")
                }
            ];
        }

        private static SKColor GetStatusColor(string statusName)
        {
            return statusName?.Trim().ToLower() switch
            {
                "н" => SKColor.Parse("#64748b"),
                "в работе" => SKColor.Parse("#3b82f6"),
                "в ожидании" => SKColor.Parse("#f49e0b"),
                "решена" => SKColor.Parse("#23c55e"),
                "закрыта" => SKColor.Parse("#9C27B0"),
                _ => SKColor.Parse("#BDBDBD")
            };
        }

        public async SystemTask ExportExcelAsync(string filePath)
        {
            if (SelectedEmployee == null) return;

            try
            {
                IsLoading = true;

                var stats = new StatisticsDto
                {
                    TotalTasks = this.TotalTasks,
                    CompletedTasks = this.CompletedTasks,
                    InProgressTasks = this.InProgressTasks,
                    ProgressPercentage = this.ProgressPercentage
                };

                var distribution = StatusDistribution.ToList();
                var employeeTasksList = EmployeeTasks.ToList();
                var employeeName = SelectedEmployee.FullName;
                var dateRange = $"{StartDate:dd.MM.yyyy} — {EndDate:dd.MM.yyyy}";

                await _exportService.ExportToExcelAsync(
                    filePath, stats, distribution,
                    employeeTasksList, employeeName, dateRange);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Excel export error: " + ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async SystemTask ExportPdfAsync(string filePath)
        {
            if (SelectedEmployee == null) return;

            try
            {
                IsLoading = true;

                var stats = new StatisticsDto
                {
                    TotalTasks = this.TotalTasks,
                    CompletedTasks = this.CompletedTasks,
                    InProgressTasks = this.InProgressTasks,
                    ProgressPercentage = this.ProgressPercentage
                };

                var distribution = StatusDistribution.ToList();
                var employeeTasksList = EmployeeTasks.ToList();
                var employeeName = SelectedEmployee.FullName;
                var dateRange = $"{StartDate:dd.MM.yyyy} — {EndDate:dd.MM.yyyy}";

                await _exportService.ExportToPdfAsync(
                    filePath, stats, distribution,
                    employeeTasksList, employeeName, dateRange);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PDF export error: " + ex);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}