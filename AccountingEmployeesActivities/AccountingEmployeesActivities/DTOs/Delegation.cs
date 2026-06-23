using System;
using System.Collections.Generic;
using System.Text;

namespace AccountingEmployeesActivities.DTOs
{
    namespace AccountingEmployeesActivities.DTOs
    {
        public class DelegatedTaskDto
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
            public string CreationDate { get; set; }
            public string Deadline { get; set; }
            public string DelegatedTo { get; set; } // Имя исполнителя
            public string Reason { get; set; } // Комментарий из таблицы executor
        }
    }
}
