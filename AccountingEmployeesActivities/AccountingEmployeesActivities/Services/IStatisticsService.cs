using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AccountingEmployeesActivities.DTOs;

namespace AccountingEmployeesActivities.Services.Interfaces
{
    public interface IStatisticsService
    {
        /// <summary>
        /// Получить общую статистику по задачам
        /// </summary>
        /// <param name="employeeId">ID сотрудника (null = все сотрудники отдела)</param>
        Task<StatisticsDto> GetStatisticsAsync(int? employeeId = null);

        /// <summary>
        /// Получить распределение задач по статусам
        /// </summary>
        Task<List<StatusDistributionDto>> GetStatusDistributionAsync(int? employeeId = null);

        /// <summary>
        /// Получить задачи по сотрудникам
        /// </summary>
        Task<List<EmployeeTasksDto>> GetEmployeeTasksAsync(int? employeeId = null);

        /// <summary>
        /// Получить список сотрудников для фильтра
        /// </summary>
        Task<List<EmployeeFilterDto>> GetEmployeesForFilterAsync(int currentUserId);
    }
}