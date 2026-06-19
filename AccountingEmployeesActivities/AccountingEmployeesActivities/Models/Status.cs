using System;
using System.Collections.Generic;

namespace AccountingEmployeesActivities.Models;

public partial class Status
{
    public int IdStatus { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
