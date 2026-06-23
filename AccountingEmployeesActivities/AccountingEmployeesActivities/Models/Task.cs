using System;
using System.Collections.Generic;

namespace AccountingEmployeesActivities.Models;

public partial class Task
{
    public int IdTask { get; set; }

    public int IdStatus { get; set; }

    public int IdCreator { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly CreationDate { get; set; }

    public DateOnly? CompletionDate { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Executor> Executors { get; set; } = new List<Executor>();

    public virtual ICollection<File> Files { get; set; } = new List<File>();

    public virtual Employee IdCreatorNavigation { get; set; } = null!;

    public virtual Status IdStatusNavigation { get; set; } = null!;
}
