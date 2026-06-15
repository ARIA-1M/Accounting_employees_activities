using System;
using System.Collections.Generic;

namespace AccountingEmployeesActivities.Models;

public partial class Comment
{
    public int IdComment { get; set; }

    public int IdTask { get; set; }

    public int IdUser { get; set; }

    public string Text { get; set; } = null!;

    public DateOnly AddDate { get; set; }

    public virtual Task IdTaskNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
