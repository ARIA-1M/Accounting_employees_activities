using AccountingEmployeesActivities.DTOs;
using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.Services.Interfaces;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace AccountingEmployeesActivities.Services.Implementations
{
    public class StatisticsService : IStatisticsService
    {
        private readonly PostgresContext _context;
        private int _currentUserId;

        private int? _completedStatusId;
        private int? _inProgressStatusId;

        public StatisticsService(PostgresContext context)
        {
            _context = context;
        }

        private async Task InitializeStatusIdsAsync()
        {
            if (_completedStatusId.HasValue && _inProgressStatusId.HasValue)
                return;

            var statuses = await _context.Statuses.ToListAsync();
            _completedStatusId = statuses.FirstOrDefault(s => s.IdStatus == 4)?.IdStatus ?? 0;
            _inProgressStatusId = statuses.FirstOrDefault(s => s.IdStatus == 3)?.IdStatus ?? 0;

            System.Diagnostics.Debug.WriteLine($"StatusIds loaded - Completed: {_completedStatusId}, InProgress: {_inProgressStatusId}");
        }

        //  Теперь Task<T> указывает на System.Threading.Tasks.Task<T>
        public async Task<StatisticsDto> GetStatisticsAsync(int? employeeId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            await InitializeStatusIdsAsync();

            //  Models.Task для модели
            var query = _context.Tasks
                .Include(t => t.IdStatusNavigation)
                .Include(t => t.Executors)
                .AsQueryable();

            //  Правильное преобразование DateOnly в DateTime для сравнения
            var actualStartDate = startDate?.Date ?? DateTime.Now.AddMonths(-1).Date;
            var actualEndDate = (endDate?.Date ?? DateTime.Now.Date).AddDays(1).AddTicks(-1); // До конца дня

            query = query.Where(t => t.CreationDate >= DateOnly.FromDateTime(actualStartDate) &&
                                      t.CreationDate <= DateOnly.FromDateTime(actualEndDate));

            if (employeeId.HasValue)
            {
                if (employeeId.Value == -1)
                {
                    var currentEmployee = await _context.Employees
                        .FirstOrDefaultAsync(e => e.IdUser == _currentUserId);

                    if (currentEmployee == null)
                        return new StatisticsDto();

                    var subordinateIds = await _context.Employees
                        .Where(e => e.IdBoss == currentEmployee.IdEmployee && e.IsActive)
                        .Select(e => e.IdEmployee)
                        .ToListAsync();

                    subordinateIds.Add(currentEmployee.IdEmployee);

                    query = query.Where(t => t.Executors.Any(e =>
                        subordinateIds.Contains(e.IdEmployee)));
                }
                else if (employeeId.Value == -2)
                {
                    var currentEmployee = await _context.Employees
                        .FirstOrDefaultAsync(e => e.IdUser == _currentUserId);

                    if (currentEmployee == null || !currentEmployee.IdBoss.HasValue)
                        return new StatisticsDto();

                    var departmentIds = await _context.Employees
                        .Where(e => e.IdBoss == currentEmployee.IdBoss && e.IsActive)
                        .Select(e => e.IdEmployee)
                        .ToListAsync();

                    query = query.Where(t => t.Executors.Any(e =>
                        departmentIds.Contains(e.IdEmployee)));
                }
                else if (employeeId.Value > 0)
                {
                    query = query.Where(t => t.Executors.Any(e =>
                        e.IdEmployee == employeeId.Value));
                }
            }

            var totalTasks = await query.CountAsync();

            var completedTasks = await query.CountAsync(t => t.IdStatus == _completedStatusId);
            var inProgressTasks = await query.CountAsync(t => t.IdStatus == _inProgressStatusId);

            var progressPercentage = totalTasks > 0
                ? (int)Math.Round((double)completedTasks / totalTasks * 100)
                : 0;

            System.Diagnostics.Debug.WriteLine($"Stats - Total: {totalTasks}, Completed: {completedTasks}, InProgress: {inProgressTasks}, Progress: {progressPercentage}%");

            return new StatisticsDto
            {
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                InProgressTasks = inProgressTasks,
                ProgressPercentage = progressPercentage
            };
        }

        public async Task<List<StatusDistributionDto>> GetStatusDistributionAsync(int? employeeId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            await InitializeStatusIdsAsync();

            var query = _context.Tasks
                .Include(t => t.IdStatusNavigation)
                .Include(t => t.Executors)
                .AsQueryable();

            var actualStartDate = startDate?.Date ?? DateTime.Now.AddMonths(-1).Date;
            var actualEndDate = (endDate?.Date ?? DateTime.Now.Date).AddDays(1).AddTicks(-1);

            query = query.Where(t => t.CreationDate >= DateOnly.FromDateTime(actualStartDate) &&
                                      t.CreationDate <= DateOnly.FromDateTime(actualEndDate));

            if (employeeId.HasValue)
            {
                if (employeeId.Value == -1 || employeeId.Value == -2)
                {
                    var currentEmployee = await _context.Employees
                        .FirstOrDefaultAsync(e => e.IdUser == _currentUserId);

                    if (currentEmployee == null)
                        return new List<StatusDistributionDto>();

                    List<int> departmentIds;

                    if (employeeId.Value == -1)
                    {
                        departmentIds = await _context.Employees
                            .Where(e => e.IdBoss == currentEmployee.IdEmployee && e.IsActive)
                            .Select(e => e.IdEmployee)
                            .ToListAsync();
                        departmentIds.Add(currentEmployee.IdEmployee);
                    }
                    else
                    {
                        if (!currentEmployee.IdBoss.HasValue)
                            return new List<StatusDistributionDto>();

                        departmentIds = await _context.Employees
                            .Where(e => e.IdBoss == currentEmployee.IdBoss && e.IsActive)
                            .Select(e => e.IdEmployee)
                            .ToListAsync();
                    }

                    query = query.Where(t => t.Executors.Any(e =>
                        departmentIds.Contains(e.IdEmployee)));
                }
                else if (employeeId.Value > 0)
                {
                    query = query.Where(t => t.Executors.Any(e =>
                        e.IdEmployee == employeeId.Value));
                }
            }

            var distribution = await query
                .GroupBy(t => new { t.IdStatus, t.IdStatusNavigation.Name })
                .Select(g => new StatusDistributionDto
                {
                    StatusName = g.Key.Name,
                    Count = g.Count(),
                    Color = GetStatusColor(g.Key.IdStatus)
                })
                .ToListAsync();

            System.Diagnostics.Debug.WriteLine($"Distribution - Total items: {distribution.Count}");

            return distribution;
        }

        public async Task<List<EmployeeTasksDto>> GetEmployeeTasksAsync(int? employeeId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            await InitializeStatusIdsAsync();

            var actualStartDate = startDate?.Date ?? DateTime.Now.AddMonths(-1).Date;
            var actualEndDate = (endDate?.Date ?? DateTime.Now.Date).AddDays(1).AddTicks(-1);

            if (employeeId == -1 || employeeId == -2)
            {
                var currentEmployee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.IdUser == _currentUserId);

                if (currentEmployee == null)
                    return new List<EmployeeTasksDto>();

                List<int> departmentIds;

                if (employeeId == -1)
                {
                    departmentIds = await _context.Employees
                        .Where(e => e.IdBoss == currentEmployee.IdEmployee && e.IsActive)
                        .Select(e => e.IdEmployee)
                        .ToListAsync();
                    departmentIds.Add(currentEmployee.IdEmployee);
                }
                else
                {
                    if (!currentEmployee.IdBoss.HasValue)
                        return new List<EmployeeTasksDto>();

                    departmentIds = await _context.Employees
                        .Where(e => e.IdBoss == currentEmployee.IdBoss && e.IsActive)
                        .Select(e => e.IdEmployee)
                        .ToListAsync();
                }

                var actualStartDateOnly = DateOnly.FromDateTime(actualStartDate);
                var actualEndDateOnly = DateOnly.FromDateTime(actualEndDate);

                var employeeTasks = await _context.Executors
                    .Where(e => departmentIds.Contains(e.IdEmployee) &&
                                e.IdTaskNavigation.CreationDate >= actualStartDateOnly &&
                                e.IdTaskNavigation.CreationDate <= actualEndDateOnly)
                    .Include(e => e.IdEmployeeNavigation)
                    .Include(e => e.IdTaskNavigation)
                        .ThenInclude(t => t.IdStatusNavigation)
                    .GroupBy(e => new
                    {
                        e.IdEmployee,
                        e.IdEmployeeNavigation.LastName,
                        e.IdEmployeeNavigation.FirstName
                    })
                    .Select(g => new EmployeeTasksDto
                    {
                        EmployeeName = g.Key.LastName + " " + g.Key.FirstName.Substring(0, 1) + ".",
                        TotalTasks = g.Count(),
                        CompletedTasks = g.Count(e => e.IdTaskNavigation.IdStatus == _completedStatusId)
                    })
                    .OrderByDescending(e => e.TotalTasks)
                    .ToListAsync();

                return employeeTasks;
            }
            else if (employeeId.HasValue && employeeId > 0)
            {
                var actualStartDateOnly = DateOnly.FromDateTime(actualStartDate);
                var actualEndDateOnly = DateOnly.FromDateTime(actualEndDate);

                var tasks = await _context.Executors
                    .Where(e => e.IdEmployee == employeeId.Value &&
                                e.IdTaskNavigation.CreationDate >= actualStartDateOnly &&
                                e.IdTaskNavigation.CreationDate <= actualEndDateOnly)
                    .Include(e => e.IdEmployeeNavigation)
                    .Include(e => e.IdTaskNavigation)
                        .ThenInclude(t => t.IdStatusNavigation)
                    .GroupBy(e => new
                    {
                        e.IdEmployee,
                        e.IdEmployeeNavigation.LastName,
                        e.IdEmployeeNavigation.FirstName
                    })
                    .Select(g => new EmployeeTasksDto
                    {
                        EmployeeName = g.Key.LastName + " " + g.Key.FirstName.Substring(0, 1) + ".",
                        TotalTasks = g.Count(),
                        CompletedTasks = g.Count(e => e.IdTaskNavigation.IdStatus == _completedStatusId)
                    })
                    .ToListAsync();

                return tasks;
            }

            return new List<EmployeeTasksDto>();
        }

        public async Task<List<EmployeeFilterDto>> GetEmployeesForFilterAsync(int currentUserId)
        {
            _currentUserId = currentUserId;

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

                employees.Insert(0, new EmployeeFilterDto
                {
                    Id = -1,
                    FullName = "Весь отдел"
                });

                employees.Insert(1, new EmployeeFilterDto
                {
                    Id = currentEmployee.IdEmployee,
                    FullName = $"Только я ({currentEmployee.FirstName})"
                });
            }
            else
            {
                employees.Add(new EmployeeFilterDto
                {
                    Id = currentEmployee.IdEmployee,
                    FullName = $"Только я ({currentEmployee.FirstName})"
                });

                if (currentEmployee.IdBoss.HasValue)
                {
                    employees.Add(new EmployeeFilterDto
                    {
                        Id = -2,
                        FullName = "Весь отдел"
                    });
                }
            }

            return employees;
        }

        private static string GetStatusColor(int idStatus)
        {
            return idStatus switch
            {
                1 => "#808080",
                2 => "#F5A623",                
                3 => "#4A90E2",
                4 => "#7ED321",
                5 => "OK",
                _ => "#CCCCCC"
            };
        }
    }
}