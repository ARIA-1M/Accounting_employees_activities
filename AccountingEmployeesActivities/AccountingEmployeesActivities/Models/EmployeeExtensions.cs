namespace AccountingEmployeesActivities.Models
{
    public partial class Employee
    {
        public string StatusText => IsActive ? "Активен" : "Уволен";
        public string StatusColor => IsActive ? "#4CAF50" : "#F44336";
        public string CardBackground => IsActive ? "#FFFFFF" : "#F0F0F0";
    }
}