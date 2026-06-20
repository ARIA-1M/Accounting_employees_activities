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
            var completedTasks = await query.CountAsync(t =>
                t.IdStatus == GetCompletedStatusId());

            var inProgressTasks = await query.CountAsync(t =>
                t.IdStatus == GetInProgressStatusId());

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

        public async Task<List<EmployeeFilterDto>> GetEmployeesForFilterAsync(int currentUserId)
        {
            var currentEmployee = await _context.Employees
                .Include(e => e.IdUserNavigation)
                .FirstOrDefaultAsync(e => e.IdUser == currentUserId);

            var employees = await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => new EmployeeFilterDto
                {
                    Id = e.IdEmployee,
                    FullName = e.LastName + " " + e.FirstName +
                              (e.MiddleName != null ? " " + e.MiddleName : "")
                })
                .OrderBy(e => e.FullName)
                .ToListAsync();

            // Добавить опцию "Все сотрудники отдела"
            employees.Insert(0, new EmployeeFilterDto
            {
                Id = -1,
                FullName = "Все сотрудники отдела"
            });

            // Добавить "Только я"
            if (currentEmployee != null)
            {
                employees.Insert(1, new EmployeeFilterDto
                {
                    Id = currentEmployee.IdEmployee,
                    FullName = $"Только я ({currentEmployee.FirstName})"
                });
            }

            return employees;
        }

        // Вспомогательные методы
        private int GetCompletedStatusId()
        {
            // Получить ID статуса "Решена" из БД или конфигурации
            return _context.Statuses
                .FirstOrDefault(s => s.Name == "Решена")?.IdStatus ?? 0;
        }

        private int GetInProgressStatusId()
        {
            return _context.Statuses
                .FirstOrDefault(s => s.Name == "В работе")?.IdStatus ?? 0;
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