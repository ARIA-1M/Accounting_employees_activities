using AccountingEmployeesActivities.ViewModels;
using AccountingEmployeesActivities.ViewModels.Pages;
using Avalonia.Controls;
using NUnit.Framework;
using System.Linq;
using AppTask = AccountingEmployeesActivities.Models.Task;

namespace Accounting.Tests.Integration
{
    [TestFixture]
    [NonParallelizable]
    public class MainViewModelTests : TestBase
    {
        [Test]
        public void Constructor_ShouldInitializeMenuItemsEmployee()
        {
            var user = CreateTestUserEmployee();
            var vm = new MainViewModel(user, Context);
            Assert.That(vm.MenuItems, Is.Not.Empty);
            Assert.That(vm.MenuItems.Select(m => m.Header),
                Is.EquivalentTo(new[] { "МОИ ЗАДАЧИ", "История задач", "Делегирование", "Статистика", "Настройки" }));
        }
            
        [Test]
        public void Constructor_ShouldInitializeMenuItemsBoss()
        {
            var user = CreateTestUserBoss();
            var vm = new MainViewModel(user, Context);
            Assert.That(vm.MenuItems, Is.Not.Empty);
            Assert.That(vm.MenuItems.Select(m => m.Header),
                Is.EquivalentTo(new[] { "ЗАДАЧИ", "История задач", "Статистика","Сотрудники" , "Настройки" }));
        }

        [Test]
        public void Navigate_ToTasks_ShouldSetCurrentPageToMyTasksViewModel()
        {
            var user = CreateTestUserEmployee();
            var vm = new MainViewModel(user, Context);
            var menuItem = vm.MenuItems.First(m => m.Header == "МОИ ЗАДАЧИ");
            vm.NavigateCommand.Execute(menuItem.PageType);
            Assert.That(vm.CurrentPage, Is.InstanceOf<MyTasksViewModel>());
        }
        [Test]
        public void Navigate_ToTasks_ShouldSetCurrentPageToTasksViewModel()
        {
            var user = CreateTestUserBoss();
            var vm = new MainViewModel(user, Context);
            var menuItem = vm.MenuItems.First(m => m.Header == "ЗАДАЧИ");
            vm.NavigateCommand.Execute(menuItem.PageType);
            Assert.That(vm.CurrentPage, Is.InstanceOf<TasksViewModel>());
        }

        [Test]
        public void Navigate_ToHistory_ShouldSetCurrentPageToHistoryViewModel()
        {
            var user = CreateTestUserEmployee();
            var vm = new MainViewModel(user, Context);
            var menuItem = vm.MenuItems.First(m => m.Header == "История задач");
            vm.NavigateCommand.Execute(menuItem.PageType);
            Assert.That(vm.CurrentPage, Is.InstanceOf<HistoryViewModel>());
        }

        [Test]
        public void Navigate_ToSamePage_ShouldCreateNewInstanceEachTime()
        {
            var user = CreateTestUserBoss();
            var vm = new MainViewModel(user, Context);
            var menuItem = vm.MenuItems.First(m => m.Header == "ЗАДАЧИ");
            vm.NavigateCommand.Execute(menuItem.PageType);
            var firstInstance = vm.CurrentPage;
            vm.NavigateCommand.Execute(menuItem.PageType);
            var secondInstance = vm.CurrentPage;
            Assert.That(firstInstance, Is.Not.SameAs(secondInstance));
        }

        [Test]
        public void Navigate_WithNullPageType_ShouldNotThrow()
        {
            var user = CreateTestUserEmployee();
            var vm = new MainViewModel(user, Context);
            Assert.DoesNotThrow(() => vm.NavigateCommand.Execute(null));
        }
    }
}