using AccountingEmployeesActivities.DTOs.AccountingEmployeesActivities.DTOs;
using AccountingEmployeesActivities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountingEmployeesActivities.Services
{
    public class DelegationService
    {
        private readonly PostgresContext _context;
        public DelegationService(PostgresContext context)
        {
            _context = context;
        }
        public async Task<List<DelegatedTaskDto>> GetDelegatedTasksAsync(int currentUserId)
        {
            // Ищем задачи, созданные текущим пользователем, у которых есть исполнители
            var tasks = await _context.Tasks
                .Where(t => t.IdCreator == currentUserId && t.Executors.Any())
                .Include(t => t.IdStatusNavigation)
                .Include(t => t.Executors)
                    .ThenInclude(e => e.IdEmployeeNavigation)
                .ToListAsync();

            return tasks.Select(t => {
                var executor = t.Executors.FirstOrDefault();
                return new DelegatedTaskDto
                {
                    Title = t.Name,
                    Description = t.Description,
                    Status = t.IdStatusNavigation.Name,
                    CreationDate = t.CreationDate.ToString("dd.MM.yyyy"),
                    Deadline = t.CompletionDate?.ToString("dd.MM.yyyy") ?? "Нет",
                    DelegatedTo = executor != null
                        ? $"{executor.IdEmployeeNavigation.LastName} {executor.IdEmployeeNavigation.FirstName}"
                        : "Не указан",
                    Reason = executor?.Comment ?? "Без причины"
                };
            }).ToList();
        }
    }
}
