using System;
using System.Collections.Generic;
using System.Text;

namespace AccountingEmployeesActivities.DTOs
{
    public class StatisticsDto
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int ProgressPercentage { get; set; }
    }

    public class StatusDistributionDto
    {
        public string StatusName { get; set; }
        public int Count { get; set; }
        public string Color { get; set; }
    }

    public class EmployeeTasksDto
    {
        public string EmployeeName { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
    }

    public class EmployeeFilterDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int? IdGlpi { get; set; }     // ID в GLPI (из User)

    }
}
