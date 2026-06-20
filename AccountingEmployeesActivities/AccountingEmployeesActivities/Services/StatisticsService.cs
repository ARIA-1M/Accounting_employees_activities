using AccountingEmployeesActivities.DTOs;
using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountingEmployeesActivities.Services.Implementations
{
    public class StatisticsService : IStatisticsService
    {
        private readonly PostgresContext _context; // Ваш DbContext

        public StatisticsService(PostgresContext context)
        {
            _context = context;
        }

        public async Task<StatisticsDto> GetStatisticsAsync(int? employeeId = null)
        {
            var query = _context.Tasks.AsQueryable();

            // Фильтрация по сотруднику
            if (employeeId.HasValue)
            {
                query = query.Where(t => t.Executors.Any(e =>
                    e.IdEmployee == employeeId.Value && e.IsActive));
            }
            else
            {
                // Получить задачи всего отдела текущего пользователя
                // Это зависит от вашей бизнес-логики
                var currentEmployee = await GetCurrentEmployeeAsync();
                var departmentEmployees = await GetDepartmentEmployeesAsync(currentEmployee.IdBoss);

                query = query.Where(t => t.Executors.Any(e =>
                    departmentEmployees.Contains(e.IdEmployee) && e.IsActive));
            }

            var totalTasks = await query.CountAsync();

            // Предполагаем, что статус "Решена" имеет определенный ID
            // Это нужно адаптировать под вашу БД
            var completedTasks = await GetCompletedStatusIdAsync();

            var inProgressTasks = await GetInProgressStatusIdAsync();

            var progressPercentage = totalTasks > 0
                ? (int)Math.Round((double)completedTasks / totalTasks * 100)
                : 0;

            return new StatisticsDto
            {
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                InProgressTasks = inProgressTasks,
                ProgressPercentage = progressPercentage
            };
        }

        public async Task<List<StatusDistributionDto>> GetStatusDistributionAsync(int? employeeId = null)
        {
            var query = _context.Set<AccountingEmployeesActivities.Models.Task>()
                .Include(t => t.Status)
                .AsQueryable();

            if (employeeId.HasValue)
            {
                query = query.Where(t => t.Executors.Any(e =>
                    e.IdEmployee == employeeId.Value && e.IsActive));
            }

            var distribution = await query
                .GroupBy(t => new { t.IdStatus, t.Status.Name })
                .Select(g => new StatusDistributionDto
                {
                    StatusName = g.Key.Name,
                    Count = g.Count(),
                    Color = GetStatusColor(g.Key.Name)
                })
                .ToListAsync();

            return distribution;
        }
        public async Task<List<EmployeeFilterDto>> GetEmployeesForFilterAsync(int currentUserId)
        {
            // Получить текущего сотрудника
            var currentEmployee = await _context.Employees
                .Include(e => e.IdUserNavigation)
                    .ThenInclude(u => u.IdRoleNavigation)
                .FirstOrDefaultAsync(e => e.IdUser == currentUserId);

            if (currentEmployee == null)
            {
                return new List<EmployeeFilterDto>();
            }

            var employees = new List<EmployeeFilterDto>();

            var hasSubordinates = await _context.Employees
                .AnyAsync(e => e.IdBoss == currentEmployee.IdEmployee && e.IsActive);

            bool isManager = hasSubordinates;

            if (isManager)
            {
                // Для руководителя: показать всех подчиненных + себя
                var subordinates = await _context.Employees
                    .Where(e => e.IdBoss == currentEmployee.IdEmployee && e.IsActive)
                    .OrderBy(e => e.LastName)
                        .ThenBy(e => e.FirstName)
                    .Select(e => new EmployeeFilterDto
                    {
                        Id = e.IdEmployee,
                        FullName = e.LastName + " " + e.FirstName +
                                  (e.MiddleName != null ? " " + e.MiddleName : "")
                    })
                    .ToListAsync();

                employees.AddRange(subordinates);

                // Добавить опцию "Весь отдел"
                employees.Insert(0, new EmployeeFilterDto
                {
                    Id = -1,
                    FullName = "Весь отдел"
                });

                // Добавить опцию "Только я"
                employees.Insert(1, new EmployeeFilterDto
                {
                    Id = currentEmployee.IdEmployee,
                    FullName = $"Только я ({currentEmployee.FirstName})"
                });
            }
            else
            {
                // Для обычного сотрудника: только две опции

                // 1. Только я
                employees.Add(new EmployeeFilterDto
                {
                    Id = currentEmployee.IdEmployee,
                    FullName = $"Только я ({currentEmployee.FirstName})"
                });

                // 2. Весь отдел (все сотрудники с тем же руководителем)
                if (currentEmployee.IdBoss.HasValue)
                {
                    employees.Add(new EmployeeFilterDto
                    {
                        Id = -2,  // Специальный ID для "весь отдел"
                        FullName = "Весь отдел"
                    });
                }
            }

            return employees;
        }
        public async Task<List<EmployeeTasksDto>> GetEmployeeTasksAsync(int? employeeId = null)
        {
            var query = _context.Executors
                .Where(e => e.IsActive)
                .Include(e => e.IdEmployeeNavigation)
                .Include(e => e.IdTaskNavigation)
                .ThenInclude(t => t.Status)
                .AsQueryable();

            if (employeeId.HasValue)
            {
                query = query.Where(e => e.IdEmployee == employeeId.Value);
            }

            var employeeTasks = await query
                .GroupBy(e => new
                {
                    e.IdEmployee,
                    FullName = e.IdEmployeeNavigation.LastName + " " +
                               e.IdEmployeeNavigation.FirstName.Substring(0, 1) + "."
                })
                .Select(g => new EmployeeTasksDto
                {
                    EmployeeName = g.Key.FullName,
                    TotalTasks = g.Count(),
                    CompletedTasks = g.Count(e => e.IdTaskNavigation.IdStatus == GetCompletedStatusId())
                })
                .OrderByDescending(e => e.TotalTasks)
                .Take(10) // Топ 10 сотрудников
                .ToListAsync();

            return employeeTasks;
        }

        // Вспомогательные методы
        private int GetCompletedStatusId()
        {
            // Получить ID статуса "Решена" из БД или конфигурации
            return _context.Statuses
                .FirstOrDefault(s => s.Name == "Решена")?.IdStatus ?? 0;
        }

        private async Task<int> GetInProgressStatusIdAsync()
        {
            var status = await _context.Statuses
                .FirstOrDefaultAsync(s => s.Name == "В работе");
            return status?.IdStatus ?? 0;
        }

        private async Task<int> GetCompletedStatusIdAsync()
        {
            var status = await _context.Statuses
                .FirstOrDefaultAsync(s => s.Name == "Решена");
            return status?.IdStatus ?? 0;
        }

        private string GetStatusColor(string statusName)
        {
            return statusName switch
            {
                "Новая" => "#808080",      // Серый
                "В работе" => "#4A90E2",   // Синий
                "Ожидание" => "#F5A623",   // Оранжевый
                "Решена" => "#7ED321",     // Зеленый
                _ => "#CCCCCC"
            };
        }

        private async Task<Employee> GetCurrentEmployeeAsync()
        {
            // Получить текущего сотрудника из контекста безопасности
            // Это зависит от вашей реализации аутентификации
            throw new NotImplementedException();
        }

        private async Task<List<int>> GetDepartmentEmployeesAsync(int? bossId)
        {
            if (!bossId.HasValue)
                return new List<int>();

            return await _context.Employees
                .Where(e => e.IdBoss == bossId && e.IsActive)
                .Select(e => e.IdEmployee)
                .ToListAsync();
        }
    }
}