using System;
using System.Collections.Generic;

namespace AccountingEmployeesActivities.Models;

public partial class User
{
    public int IdUser { get; set; }

    public int IdRole { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? IdGlpi { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual Employee? Employee { get; set; }

    public virtual Role IdRoleNavigation { get; set; } = null!;
}
