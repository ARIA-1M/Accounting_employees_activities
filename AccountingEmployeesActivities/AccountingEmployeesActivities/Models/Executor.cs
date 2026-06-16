using System;
using System.Collections.Generic;

namespace AccountingEmployeesActivities.Models;

public partial class Executor
{
    public int IdExecutor { get; set; }

    public int IdTask { get; set; }

    public int IdEmployee { get; set; }

    public bool? IsActive { get; set; }

    public string? Comment { get; set; }

    public DateOnly? ChangeDate { get; set; }

    public virtual Employee IdEmployeeNavigation { get; set; } = null!;

    public virtual Task IdTaskNavigation { get; set; } = null!;
}
