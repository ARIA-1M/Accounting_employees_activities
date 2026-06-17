using System;
using System.Collections.Generic;

namespace AccountingEmployeesActivities.Models;

public partial class Employee
{
    public int IdEmployee { get; set; }
    public int? IdUser { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public DateOnly? BirthDate { get; set; }

    public int? IdBoss { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Executor> Executors { get; set; } = new List<Executor>();

    public virtual Employee? IdBossNavigation { get; set; }

    public virtual User? IdUserNavigation { get; set; }


    public virtual ICollection<Employee> InverseIdBossNavigation { get; set; } = new List<Employee>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
