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
        /// Получить список сотрудников для фильтра
        /// </summary>
        Task<List<EmployeeFilterDto>> GetEmployeesForFilterAsync(int currentUserId);

        // ДОБАВЛЕНЫ ПАРАМЕТРЫ ДАТА
        Task<StatisticsDto> GetStatisticsAsync(int? employeeId = null, DateTime? startDate = null, DateTime? endDate = null);

        Task<List<StatusDistributionDto>> GetStatusDistributionAsync(int? employeeId = null, DateTime? startDate = null, DateTime? endDate = null);

        Task<List<EmployeeTasksDto>> GetEmployeeTasksAsync(int? employeeId = null, DateTime? startDate = null, DateTime? endDate = null);

    }

}
