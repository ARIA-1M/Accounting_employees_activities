using System;
using System.Collections.Generic;
using System.Text;
using AccountingEmployeesActivities.DTOs;
using global::AccountingEmployeesActivities.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountingEmployeesActivities.Services
{


    public interface IExportService
    {
        Task ExportToExcelAsync(
            string filePath,
            StatisticsDto statistics,
            List<StatusDistributionDto> statusDistribution,
            List<EmployeeTasksDto> employeeTasks,
            string employeeName,
            string dateRange);

        Task ExportToPdfAsync(
            string filePath,
            StatisticsDto statistics,
            List<StatusDistributionDto> statusDistribution,
            List<EmployeeTasksDto> employeeTasks,
            string employeeName,
            string dateRange);
    }
}
