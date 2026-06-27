using AccountingEmployeesActivities.ViewModels;
using AccountingEmployeesActivities.ViewModels.Pages;
using NUnit.Framework;
using ReactiveUI;
using System;
using System.Linq;
using AppTask = AccountingEmployeesActivities.Models.Task;

namespace Accounting.Tests.Integration
{
    [TestFixture]
    [NonParallelizable]
    public class HistoryViewModelTests : TestBase
    {
        [Test]
        public void LoadCompletedTasks_ForEmployee_ShouldLoadOnlyResolvedTasksForHim()
        {
            var user = CreateTestUserEmployee("emp", "Employee");
            var employee = CreateTestEmployee(user);

            var resolvedTask = CreateTestTask("Resolved", statusId: 4, completionDate: DateOnly.FromDateTime(DateTime.Now));
            var openTask = CreateTestTask("Open", statusId: 1);
            CreateTestExecutor(resolvedTask, employee);
            CreateTestExecutor(openTask, employee);

            var vm = new HistoryViewModel(user, Context, isBoss: false);

            Assert.That(vm.CompletedTasks, Has.Count.EqualTo(1));
            var task = vm.CompletedTasks.First();
            Assert.That(task.Name, Is.EqualTo("Resolved"));
            Assert.That(task.Status, Is.EqualTo("Решена"));

        }

        [Test]
        public void LoadCompletedTasks_ForBoss_ShouldLoadAllResolvedTasks2()
        {
            // Создаём босса (пользователь + сотрудник)
            var (userBoss, bossEmployee) = CreateTestUserAndEmployee("boss", "Boss");

            // Создаём двух подчинённых (исполнителей) без указания босса (NULL допустим)
            var (_, emp1) = CreateTestUserAndEmployee("emp1", "Employee");
            var (_, emp2) = CreateTestUserAndEmployee("emp2", "Employee");

            // Создаём задачи, указывая корректного создателя (сотрудник-босс)
            var task1 = CreateTestTask("Resolved1", statusId: 4, completionDate: DateOnly.FromDateTime(DateTime.Now), idCreator: bossEmployee.IdEmployee);
            var task2 = CreateTestTask("Resolved2", statusId: 4, completionDate: DateOnly.FromDateTime(DateTime.Now), idCreator: bossEmployee.IdEmployee);

            // Назначаем исполнителей
            CreateTestExecutor(task1, emp1);
            CreateTestExecutor(task2, emp2);

            var vm = new HistoryViewModel(userBoss, Context, isBoss: true);

            Assert.That(vm.CompletedTasks, Has.Count.EqualTo(2));
            Assert.That(vm.CompletedTasks.All(t => t.Status == "Решена"), Is.True); 
        }

        [Test]
        public void LoadCompletedTasks_ShouldOrderByCompletionDateDescending()
        {
            var user = CreateTestUserEmployee();
            var employee = CreateTestEmployee(user);

            var taskOld = CreateTestTask("Old", statusId: 4, completionDate: DateOnly.FromDateTime(DateTime.Now.AddDays(-5)));
            var taskNew = CreateTestTask("New", statusId: 4, completionDate: DateOnly.FromDateTime(DateTime.Now));
            CreateTestExecutor(taskOld, employee);
            CreateTestExecutor(taskNew, employee);

            var vm = new HistoryViewModel(user, Context, isBoss: false);

            Assert.That(vm.CompletedTasks.Count, Is.EqualTo(2));
            Assert.That(vm.CompletedTasks.First().Name, Is.EqualTo("New"));
            Assert.That(vm.CompletedTasks.Last().Name, Is.EqualTo("Old"));
        }

        [Test]
        public void LoadCompletedTasks_WhenNoTasks_ShouldReturnEmptyList()
        {
            var user = CreateTestUserEmployee();
            var employee = CreateTestEmployee(user);
            var vm = new HistoryViewModel(user, Context, isBoss: false);
            Assert.That(vm.CompletedTasks, Is.Empty);
        }

        [Test]
        public void LoadCompletedTasks_ShouldNotIncludeTasksWithNullCompletionDate()
        {
            var user = CreateTestUserEmployee();
            var employee = CreateTestEmployee(user);

            var resolved = CreateTestTask("Resolved", statusId: 4, completionDate: DateOnly.FromDateTime(DateTime.Now));
            var notCompleted = CreateTestTask("NotCompleted", statusId: 4, completionDate: null);
            CreateTestExecutor(resolved, employee);
            CreateTestExecutor(notCompleted, employee);

            var vm = new HistoryViewModel(user, Context, isBoss: false );

            Assert.That(vm.CompletedTasks, Has.Count.EqualTo(1));
            Assert.That(vm.CompletedTasks.First().Name, Is.EqualTo("Resolved"));
        }

        [Test]
        public void LoadCompletedTasks_ShouldShowCorrectStatusText()
        {
            var user = CreateTestUserEmployee();
            var employee = CreateTestEmployee(user);
            var task = CreateTestTask("Task", statusId: 4, completionDate: DateOnly.FromDateTime(DateTime.Now));
            CreateTestExecutor(task, employee);

            var vm = new HistoryViewModel(user, Context, isBoss: false);

            Assert.That(vm.CompletedTasks.First().Status, Is.EqualTo("Решена"));
        }
    }
}