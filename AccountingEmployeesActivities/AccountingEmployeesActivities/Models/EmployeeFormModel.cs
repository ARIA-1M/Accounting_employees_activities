using System;
using System.Collections.Generic;
using System.Text;

namespace AccountingEmployeesActivities.Models
{
    public class EmployeeFormModel
    {
        // Данные пользователя
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Данные сотрудника
        public int? IdEmployee { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;

        // Роль
        public int IdRole { get; set; }
        public bool IsActive { get; set; } = true;
        public int IdBoss { get; set; }
    }
}
