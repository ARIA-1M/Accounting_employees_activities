// Services/Interfaces/IGlpiService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AccountingEmployeesActivities.Services.Models;

namespace AccountingEmployeesActivities.Services.Interfaces
{
    public interface IGlpiService
    {
        Task<string> GetSessionToken();
        Task<List<GlpiTicket>> GetTicketsAsync(DateTime startDate, DateTime endDate, int? employeeId = null);
        Task<GlpiSearchResponse> SearchTicketsAsync(DateTime startDate, DateTime endDate, int? employeeId = null);
        Task<List<GlpiTicket>> GetTicketsByEmployeeAsync(int employeeId, DateTime startDate, DateTime endDate);
    }
}