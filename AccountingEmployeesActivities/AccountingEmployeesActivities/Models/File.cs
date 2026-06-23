using System;
using System.Collections.Generic;

namespace AccountingEmployeesActivities.Models;

public partial class File
{
    public int IdFile { get; set; }

    public int IdTask { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly AddDate { get; set; }

    public virtual Task IdTaskNavigation { get; set; } = null!;
}
