using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.ViewModels;
using NUnit.Framework;
using System;
using System.Linq;
using AppTask = AccountingEmployeesActivities.Models.Task;

namespace Accounting.Tests.Integration
{
    [TestFixture]
    [NonParallelizable]
    public class CommentViewModelTests : TestBase
    {
        [Test]
        public void Constructor_ShouldLoadExistingComments()
        {
            var user = CreateTestUserEmployee();
            var task = CreateTestTask();
            var comment = CreateTestComment(task, user, "Hello");

            var vm = new CommentViewModel(task.IdTask, user.IdUser, Context);

            Assert.That(vm.Comments, Has.Count.EqualTo(1));
            var loaded = vm.Comments.First();
            Assert.That(loaded.Text, Is.EqualTo("Hello"));
            Assert.That(loaded.UserName, Is.EqualTo(user.Login));
        }


        [Test]
        public void SendComment_WithEmptyText_ShouldNotAddComment()
        {
            var user = CreateTestUserEmployee();
            var task = CreateTestTask();
            var vm = new CommentViewModel(task.IdTask, user.IdUser, Context);
            vm.NewComment = "";

            vm.SendCommentCommand.Execute(null);

            Assert.That(vm.Comments, Is.Empty);
            var saved = Context.Comments.FirstOrDefault(c => c.Text == "");
            Assert.That(saved, Is.Null);
        }

        [Test]
        public void SendComment_WithWhitespace_ShouldNotAddComment()
        {
            var user = CreateTestUserEmployee();
            var task = CreateTestTask();
            var vm = new CommentViewModel(task.IdTask, user.IdUser, Context);
            vm.NewComment = "   ";

            vm.SendCommentCommand.Execute(null);

            Assert.That(vm.Comments, Is.Empty);
        }

        [Test]
        public void LoadComments_ShouldLoadBothComments()
        {
            var user = CreateTestUserEmployee();
            var task = CreateTestTask();

            var oldComment = CreateTestComment(task, user, "Old");
            var oldEntity = Context.Comments.First(c => c.Text == "Old");
            oldEntity.AddDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-2));
            Context.SaveChanges();

            var newComment = CreateTestComment(task, user, "New");
            var newEntity = Context.Comments.First(c => c.Text == "New");
            newEntity.AddDate = DateOnly.FromDateTime(DateTime.Now);
            Context.SaveChanges();

            var vm = new CommentViewModel(task.IdTask, user.IdUser, Context);

            Assert.That(vm.Comments, Has.Count.EqualTo(2));
            var texts = vm.Comments.Select(c => c.Text).ToList();
            Assert.That(texts, Is.EquivalentTo(new[] { "Old", "New" })); // порядок не важен
        }
    }
}