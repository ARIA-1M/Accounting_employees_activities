using AccountingEmployeesActivities.Models;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace AccountingEmployeesActivities.ViewModels
{
    public class CommentViewModel : ViewModelBase
    {
        private readonly int _taskId;
        private readonly int _currentUserId;
        private string _newComment = string.Empty;

        public ObservableCollection<CommentItem> Comments { get; }
        public ICommand SendCommentCommand { get; }

        public string NewComment
        {
            get => _newComment;
            set { _newComment = value; OnPropertyChanged(); }
        }

        public CommentViewModel(int taskId, int currentUserId, PostgresContext db)
        {
            _taskId = taskId;
            _currentUserId = currentUserId;
            Comments = new ObservableCollection<CommentItem>();
            SendCommentCommand = new RelayCommand(_ => SendComment());
            LoadComments(db);
        }
        public CommentViewModel(int taskId, int currentUserId)
        {
            _taskId = taskId;
            _currentUserId = currentUserId;
            Comments = new ObservableCollection<CommentItem>();
            SendCommentCommand = new RelayCommand(_ => SendComment());
            using var db = new PostgresContext();
            LoadComments(db);
        }

        private void LoadComments(PostgresContext db)
        {
  
            // 1. Загружаем данные из БД
            var rawComments = db.Comments
                .Where(c => c.IdTask == _taskId)
                .OrderBy(c => c.AddDate)
                .Include(c => c.IdUserNavigation)
                .Select(c => new
                {
                    Login = c.IdUserNavigation.Login,
                    Text = c.Text,
                    AddDate = c.AddDate,
                    IsBoss = c.IdUserNavigation.IdRole == 2
                })
                .ToList(); // Выполняем запрос в БД

            // 2. Преобразуем строки уже в памяти
            foreach (var c in rawComments)
            {
                Comments.Add(new CommentItem
                {
                    UserName = ToTitleCase(c.Login),
                    Text = ToSentenceCase(c.Text),
                    AddDate = c.AddDate,
                    IsBoss = c.IsBoss
                });
            }
        }

        private void SendComment()
        {
            if (string.IsNullOrWhiteSpace(NewComment))
                return;

            using var db = new PostgresContext();
            var user = db.Users.Find(_currentUserId);
            bool isBoss = user?.IdRole == 2;

            var comment = new Comment
            {
                IdTask = _taskId,
                IdUser = _currentUserId,
                Text = NewComment,
                AddDate = DateOnly.FromDateTime(DateTime.Now)
            };
            db.Comments.Add(comment);
            db.SaveChanges();

            Comments.Add(new CommentItem
            {
                UserName = ToTitleCase(user?.Login ?? "Я"),
                Text = ToSentenceCase(NewComment),
                AddDate = comment.AddDate,
                IsBoss = isBoss
            });

            NewComment = string.Empty;
            OnPropertyChanged(nameof(NewComment));
        }

        // Статические методы, чтобы не захватывать экземпляр
        private static string ToTitleCase(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        private static string ToSentenceCase(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            return char.ToUpper(str[0]) + str.Substring(1).ToLower();
        }
    }

    public class CommentItem
    {
        public string UserName { get; set; }
        public string Text { get; set; }
        public DateOnly AddDate { get; set; }
        public bool IsBoss { get; set; }
        public bool IsEmployee => !IsBoss;
    }
}