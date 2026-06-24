using System;
using System.Collections.Generic;

namespace AccountingEmployeesActivities.DTOs
{
    public class TaskCardModel
    {
        public int IdTask { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string StatusText { get; set; }
        public string StatusColor { get; set; }
        public string CreatedDate { get; set; }
        public string Deadline { get; set; }
        public int CommentsCount { get; set; }
        public string LastCommentAuthor { get; set; }
        public string LastCommentText { get; set; }
        public string LastCommentDate { get; set; }
        public List<string> Files { get; set; } = new List<string>();
        public bool CanDelegate { get; set; }  // можно ли делегировать
        public bool IsDelegated { get; set; }   // делегирована ли задача
        public string DelegatedTo { get; set; } // кому делегирована
        public string DelegatedReason { get; set; } // причина делегирования
    }
}