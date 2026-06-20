using AccountingEmployeesActivities.DTOs;
using AccountingEmployeesActivities.DTOs;
using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.Services.Interfaces;
using AccountingEmployeesActivities.Services.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
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

        private ObservableCollection<EmployeeFilterDto> _employees;
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
                SetProperty(ref _selectedEmployee, value);
                _ = LoadStatisticsAsync();
            }
        }

        private ObservableCollection<StatusDistributionDto> _statusDistribution;
        public ObservableCollection<StatusDistributionDto> StatusDistribution
        {
            get => _statusDistribution;
            set => SetProperty(ref _statusDistribution, value);
        }

        private ObservableCollection<EmployeeTasksDto> _employeeTasks;
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

        public StatisticsViewModel(User currentUser, IStatisticsService statisticsService)
        {
            _currentUserId = currentUser.IdUser;
            _statisticsService = statisticsService;

            _ = InitializeAsync();
        }

        private async SystemTask InitializeAsync()
        {
            IsLoading = true;

            try
            {
                // Загрузить список сотрудников для фильтра
                var employees = await _statisticsService
                    .GetEmployeesForFilterAsync(_currentUserId);
                Employees = new ObservableCollection<EmployeeFilterDto>(employees);
                SelectedEmployee = Employees.FirstOrDefault();

                await LoadStatisticsAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async SystemTask LoadStatisticsAsync()
        {
            if (SelectedEmployee == null)
                return;

            IsLoading = true;

            try
            {
                int? employeeId = SelectedEmployee.Id == -1 || SelectedEmployee.Id == -2
                    ? SelectedEmployee.Id
                    : SelectedEmployee.Id;

                // 1. Загрузить основную статистику
                var stats = await _statisticsService.GetStatisticsAsync(employeeId);
                TotalTasks = stats.TotalTasks;
                CompletedTasks = stats.CompletedTasks;
                InProgressTasks = stats.InProgressTasks;
                ProgressPercentage = stats.ProgressPercentage;

                // 2. Загрузить распределение по статусам
                var distribution = await _statisticsService
                    .GetStatusDistributionAsync(employeeId);
                StatusDistribution = new ObservableCollection<StatusDistributionDto>(
                    distribution ?? new List<StatusDistributionDto>()
                );

                // 3. Загрузить задачи по сотрудникам
                var empTasks = await _statisticsService
                    .GetEmployeeTasksAsync(employeeId);
                EmployeeTasks = new ObservableCollection<EmployeeTasksDto>(
                    empTasks ?? new List<EmployeeTasksDto>()
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading statistics: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

    }
}