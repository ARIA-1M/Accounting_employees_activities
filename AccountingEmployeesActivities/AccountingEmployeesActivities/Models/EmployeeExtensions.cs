namespace AccountingEmployeesActivities.Models
{
    public partial class Employee
    {
        public string StatusText => IsActive.GetValueOrDefault() ? "Активен" : "Уволен";
        public string StatusColor => IsActive.GetValueOrDefault() ? "#4CAF50" : "#F44336";
        public string CardBackground => IsActive.GetValueOrDefault() ? "#FFFFFF" : "#F0F0F0";
    }
}