using AccountingEmployeesActivities.DTOs;
using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.Services.Interfaces;
using AccountingEmployeesActivities.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SystemTask = System.Threading.Tasks.Task;


namespace AccountingEmployeesActivities.Services.Implementations
{
    public class StatisticsGlpiService : IStatisticsService
    {
        private readonly IGlpiService _glpiService;

        public StatisticsGlpiService(IGlpiService glpiService)
        {
            _glpiService = glpiService;
        }

        /// <summary>
        /// Получение сводной статистики по задачам
        /// </summary>
        public async Task<StatisticsDto> GetStatisticsAsync(
            int? employeeId,
            DateTime? startDate,
            DateTime? endDate)
        {
            // Если employeeId == 0 или null -> "Все сотрудники", не передаём фильтр
            int? glpiUserId = employeeId.HasValue && employeeId.Value > 0
                ? await GetGlpiUserIdByEmployeeId(employeeId.Value)
                : null;

            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            var tickets = await _glpiService.GetTicketsAsync(start, end, glpiUserId);

            var total = tickets.Count;
            var completed = tickets.Count(t => t.Status == 5);
            var inProgress = tickets.Count(t => t.Status == 2 || t.Status == 3);

            var progress = total > 0 ? (int)Math.Round((double)completed / total * 100) : 0;

            return new StatisticsDto
            {
                TotalTasks = total,
                CompletedTasks = completed,
                InProgressTasks = inProgress,
                ProgressPercentage = progress
            };
        }

        /// <summary>
        /// Получение распределения задач по статусам
        /// </summary>
        public async Task<List<StatusDistributionDto>> GetStatusDistributionAsync(
            int? employeeId,
            DateTime? startDate,
            DateTime? endDate)
        {
            int? glpiUserId = employeeId.HasValue && employeeId.Value > 0
                ? await GetGlpiUserIdByEmployeeId(employeeId.Value)
                : null;

            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            var tickets = await _glpiService.GetTicketsAsync(start, end, glpiUserId);

            var statusMap = new Dictionary<int, string>
            {
                { 1, "Новая" },
                { 2, "В работе" },
                { 3, "В ожидании" },
                { 4, "Отложена" },
                { 5, "Решена" },
                { 6, "Закрыта" }
            };

            return tickets
                .GroupBy(t => t.Status)
                .Select(g => new StatusDistributionDto
                {
                    StatusName = statusMap.ContainsKey(g.Key) ? statusMap[g.Key] : $"Статус {g.Key}",
                    Count = g.Count()
                })
                .ToList();
        }

        /// <summary>
        /// Получение количества задач по сотрудникам
        /// </summary>
        public async Task<List<EmployeeTasksDto>> GetEmployeeTasksAsync(
            int? employeeId,
            DateTime? startDate,
            DateTime? endDate)
        {
            int? glpiUserId = employeeId.HasValue && employeeId.Value > 0
                ? await GetGlpiUserIdByEmployeeId(employeeId.Value)
                : null;

            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            var tickets = await _glpiService.GetTicketsAsync(start, end, glpiUserId);

            if (tickets == null || !tickets.Any())
            {
                return new List<EmployeeTasksDto>();
            }

            // Группируем по исполнителю (поле 5 - users_id_assign)
            return tickets
                .Where(t => t.UsersIdAssign.HasValue && t.UsersIdAssign.Value > 0)
                .GroupBy(t => t.UsersIdAssign.Value)
                .Select(g => new EmployeeTasksDto
                {
                    EmployeeName = GetEmployeeNameByGlpiId(g.Key),
                    TotalTasks = g.Count(),
                    CompletedTasks = g.Count(t => t.Status == 5)
                })
                .ToList();
        }

        /// <summary>
        /// Получение списка сотрудников для фильтра
        /// </summary>
        public async Task<List<EmployeeFilterDto>> GetEmployeesForFilterAsync(int currentUserId)
        {
            using var db = new PostgresContext();

            // Загружаем подчинённых текущего пользователя
            var employees = db.Employees
                .Include(e => e.IdUserNavigation)
                .Where(e => e.IdBoss == currentUserId)
                .Select(e => new EmployeeFilterDto
                {
                    Id = e.IdEmployee,
                    FullName = $"{e.LastName} {e.FirstName}".Trim(),
                    IdGlpi = e.IdUserNavigation != null ? e.IdUserNavigation.IdGlpi : null
                })
                .ToList();

            if (employees.Any())
            {
                employees.Insert(0, new EmployeeFilterDto { Id = 0, FullName = "Все сотрудники", IdGlpi = null });
            }

            return await SystemTask.FromResult(employees);
        }

        // ========== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ==========

        private async Task<int?> GetGlpiUserIdByEmployeeId(int employeeId)
        {
            using var db = new PostgresContext();
            var employee = await db.Employees
                .Include(e => e.IdUserNavigation)
                .FirstOrDefaultAsync(e => e.IdEmployee == employeeId);

            return employee?.IdUserNavigation?.IdGlpi;
        }

        private string GetEmployeeNameByGlpiId(int glpiId)
        {
            using var db = new PostgresContext();
            var employee = db.Employees
                .Include(e => e.IdUserNavigation)
                .FirstOrDefault(e => e.IdUserNavigation != null && e.IdUserNavigation.IdGlpi == glpiId);

            return employee != null
                ? $"{employee.LastName} {employee.FirstName}".Trim()
                : $"Сотрудник GLPI (ID {glpiId})";
        }
    }
}