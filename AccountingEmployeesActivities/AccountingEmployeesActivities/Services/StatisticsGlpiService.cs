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
        private readonly PostgresContext _db;
        public StatisticsGlpiService(IGlpiService glpiService, PostgresContext db)
        {
            _glpiService = glpiService;
            _db = db;
        }

        /// <summary>
        /// Получение сводной статистики по задачам
        /// </summary>
        public async Task<StatisticsDto> GetStatisticsAsync(int? employeeId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var actualStart = startDate ?? DateTime.Now.AddMonths(-1);
            var actualEnd = endDate ?? DateTime.Now;

            // Вызываем GLPI
            var total = await _glpiService.GetTicketCountAsync(actualStart, actualEnd, employeeId);

            //var total = tickets.Count;
            //var completed = tickets.Count(t => t.Status == 5 || t.Status == 54); // пример закрытых
            //var inProgress = tickets.Count(t => t.Status == 2 || t.Status == 3);

            return new StatisticsDto
            {
                TotalTasks = total,
                CompletedTasks = 0,  // если не парсим, то ставим 0
                InProgressTasks = total,
                ProgressPercentage = 0
            };
        }

        /// <summary>
        /// Получение распределения задач по статусам
        /// </summary>
        public async Task<List<StatusDistributionDto>> GetStatusDistributionAsync(int? employeeId = null, DateTime? startDate = null, DateTime? endDate = null)
        {

            var actualStartDate = startDate?.Date ?? DateTime.Now.AddMonths(-1).Date;
            var actualEndDate = (endDate?.Date ?? DateTime.Now.Date).AddDays(1).AddTicks(-1);

            var tickets = await _glpiService.GetTicketsAsync(actualStartDate, actualEndDate, employeeId);

            return tickets
                .GroupBy(t => t.StatusName)
                .Select(g => new StatusDistributionDto
                {
                    StatusName = g.Key,
                    Count = g.Count()
                })
                .ToList();
        }

        /// <summary>
        /// Получение количества задач по сотрудникам
        /// </summary>
        public async Task<List<EmployeeTasksDto>> GetEmployeeTasksAsync(int? employeeId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var actualStartDate = startDate?.Date ?? DateTime.Now.AddMonths(-1).Date;
            var actualEndDate = (endDate?.Date ?? DateTime.Now.Date).AddDays(1).AddTicks(-1);
            // Если фильтр по конкретному сотруднику – показываем только его задачи (или всех?)
            // Здесь возвращаем разбивку по всем сотрудникам, если не задан фильтр
            List<GlpiTicket> tickets;
            if (employeeId.HasValue)
            {
                tickets = await _glpiService.GetTicketsByEmployeeAsync(employeeId.Value, actualStartDate, actualEndDate);
                // Группируем по сотруднику (но у нас только один)
                var employeeName = await GetEmployeeName(employeeId.Value);
                return new List<EmployeeTasksDto>
                {
                    new EmployeeTasksDto
                    {
                        EmployeeName = employeeName,
                        TotalTasks = tickets.Count,
                        CompletedTasks = tickets.Count(t => t.Status == 5 || t.Status == 54)
                    }
                };
            }
            else
            {
                // Получаем все тикеты и группируем по RequesterId
                tickets = await _glpiService.GetTicketsAsync(actualStartDate, actualEndDate, null);
                var grouped = tickets.GroupBy(t => t.RequesterId);
                var result = new List<EmployeeTasksDto>();
                foreach (var group in grouped)
                {
                    var name = await GetEmployeeName(group.Key);
                    result.Add(new EmployeeTasksDto
                    {
                        EmployeeName = name,
                        TotalTasks = group.Count(),
                        CompletedTasks = group.Count(t => t.Status == 5 || t.Status == 54)
                    });
                }
                return result;
            }
        }

        /// <summary>
        /// Получение списка сотрудников для фильтра
        /// </summary>
        public async Task<List<EmployeeFilterDto>> GetEmployeesForFilterAsync(int currentEmployeeId)
        {
            // Берём сотрудников из локальной БД (Postgres)
            var employees = await _db.Employees
                .Where(e => e.IdUser == currentEmployeeId || e.IsActive == true) // своя логика
                .Select(e => new EmployeeFilterDto
                {
                    Id = e.IdEmployee,               // Предполагаем, что этот ID совпадает с ID в GLPI
                    FullName = e.FirstName
                })
                .ToListAsync();

            // Добавляем пункт "Все"
            employees.Insert(0, new EmployeeFilterDto { Id = 0, FullName = "Все сотрудники" });
            return employees;
        }

        private async Task<string> GetEmployeeName(int glpiUserId)
        {
            // Ищем в локальной БД по ID (если совпадает)
            var emp = await _db.Employees.FirstOrDefaultAsync(e => e.IdEmployee == glpiUserId);
            return emp?.FirstName ?? $"Пользователь {glpiUserId}";
        }




    }
}