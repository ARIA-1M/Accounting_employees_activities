using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AccountingEmployeesActivities.DTOs;
using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.Services.Interfaces;
using SystemTask = System.Threading.Tasks.Task;

namespace AccountingEmployeesActivities.ViewModels.Pages
{
    public class StatisticsViewModel : ViewModelBase
    {
        private readonly IStatisticsService _statisticsService;
        private readonly int _currentUserId;

        #region Properties

        private string _title = "Статистика сотрудников";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        // НОВЫЕ СВОЙСТВА ДЛЯ ФИЛЬТРА ПО ДАТЕ
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

        public StatisticsViewModel(User currentUser, IStatisticsService statisticsService)
        {
            _currentUserId = currentUser.IdUser;
            _statisticsService = statisticsService;

            RefreshCommand = new RelayCommand(_ => _ = RefreshAsync());

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
                var employees = await _statisticsService.GetEmployeesForFilterAsync(_currentUserId);
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
            if (SelectedEmployee == null) return;

            IsLoading = true;

            try
            {
                int? filterId = SelectedEmployee.Id;

                // ПЕРЕДАЕМ ДАТЫ В СЕРВИС
                var stats = await _statisticsService.GetStatisticsAsync(filterId, _startDate, _endDate);
                TotalTasks = stats.TotalTasks;
                CompletedTasks = stats.CompletedTasks;
                InProgressTasks = stats.InProgressTasks;
                ProgressPercentage = stats.ProgressPercentage;

                var distribution = await _statisticsService.GetStatusDistributionAsync(filterId, _startDate, _endDate);
                StatusDistribution = new ObservableCollection<StatusDistributionDto>(distribution ?? new());

                var employeeTasks = await _statisticsService.GetEmployeeTasksAsync(filterId, _startDate, _endDate);
                EmployeeTasks = new ObservableCollection<EmployeeTasksDto>(employeeTasks ?? new());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadStatistics error: " + ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}