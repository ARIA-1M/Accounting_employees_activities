using AccountingEmployeesActivities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NUnit.Framework;
using System;
using System.Linq;
using AppTask = AccountingEmployeesActivities.Models.Task;

namespace Accounting.Tests.Integration
{
    public abstract class TestBase
    {
        protected PostgresContext Context { get; private set; }
        private IDbContextTransaction _transaction;

        [SetUp]
        public void Setup()
        {
            var connectionString = "Host=localhost;Database=test_accounting;Username=postgres;Password=yourpassword";
            var options = new DbContextOptionsBuilder<PostgresContext>()
                .UseNpgsql(connectionString)
                .Options;

            Context = new PostgresContext(options);
            _transaction = Context.Database.BeginTransaction();
        }

        [TearDown]
        public void TearDown()
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
            Context?.Dispose();
        }

        // Создание роли, пользователя и сотрудника (возвращаем пользователя и сотрудника)
        protected (User user, Employee employee) CreateTestUserAndEmployee(string login = "Testuser", string roleName = "Employee", string firstName = "Test", string lastName = "User")
        {
            var role = Context.Roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null)
            {
                role = new Role { Name = roleName };
                Context.Roles.Add(role);
                Context.SaveChanges();
            }

            var user = new User { Login = login, Password = "pass", IdRole = role.IdRole };
            Context.Users.Add(user);
            Context.SaveChanges();

            var employee = new Employee
            {
                IdUser = user.IdUser,
                FirstName = firstName,
                LastName = lastName,
                MiddleName = null,
                IdBoss = null,
                IsActive = true
            };
            Context.Employees.Add(employee);
            Context.SaveChanges();

            return (user, employee);
        }
        // Для обратной совместимости: создаём только пользователя (если нужен только пользователь)
        protected User CreateTestUserBoss(string login = "testuser", string roleName = "руководитель")
        {
            var role = Context.Roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null)
            {
                role = new Role { Name = roleName };
                Context.Roles.Add(role);
                Context.SaveChanges();
            }

            var user = new User { Login = login, Password = "pass", IdRole = role.IdRole };
            Context.Users.Add(user);
            Context.SaveChanges();
            return user;
        }
        // Для обратной совместимости: создаём только пользователя (если нужен только пользователь)
        protected User CreateTestUserEmployee(string login = "Testuser", string roleName = "сотрудник")
        {
            var role = Context.Roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null)
            {
                role = new Role { Name = roleName };
                Context.Roles.Add(role);
                Context.SaveChanges();
            }

            var user = new User { Login = login, Password = "pass", IdRole = role.IdRole };
            Context.Users.Add(user);
            Context.SaveChanges();
            return user;
        }

        // Создание сотрудника (если нужен без пользователя, но лучше использовать CreateTestUserAndEmployee)
        protected Employee CreateTestEmployee(User user, string firstName = "Test", string lastName = "Employee", int idBoss = 1)
        {
            var employee = new Employee
            {
                IdUser = user.IdUser,
                FirstName = firstName,
                LastName = lastName,
                MiddleName = null,
                IdBoss = idBoss,
                IsActive = true
            };
            Context.Employees.Add(employee);
            Context.SaveChanges();
            return employee;
        }

        // Создание задачи (теперь обязательно указываем idCreator)
        protected AppTask CreateTestTask(string title = "Test Task", int statusId = 1, DateOnly? completionDate = null, int idCreator = 1)
        {
            if (idCreator == 0)
                throw new ArgumentException("idCreator must be provided (valid Employee.IdEmployee)");

            var task = new AppTask
            {
                Name = title,          
                IdStatus = statusId,
                IdCreator = idCreator,
                Description = null,
                CreationDate = DateOnly.FromDateTime(DateTime.Now),
                CompletionDate = completionDate
            };
            Context.Tasks.Add(task);
            Context.SaveChanges();
            return task;
        }

        // Создание исполнителя
        protected Executor CreateTestExecutor(AppTask task, Employee employee, bool isActive = true, string comment = null, DateOnly? changeDate = null)
        {
            var executor = new Executor
            {
                IdTask = task.IdTask,
                IdEmployee = employee.IdEmployee,
                IsActive = isActive,
                Comment = comment,
                ChangeDate = changeDate ?? DateOnly.FromDateTime(DateTime.Now)
            };
            Context.Executors.Add(executor);
            Context.SaveChanges();
            return executor;
        }

        // Создание комментария (в схеме add_date, свойство модели AddDate)
        protected Comment CreateTestComment(AppTask task, User user, string text = "Test comment")
        {
            var comment = new Comment
            {
                IdTask = task.IdTask,
                IdUser = user.IdUser,
                Text = text,
                AddDate = DateOnly.FromDateTime(DateTime.Now)
            };
            Context.Comments.Add(comment);
            Context.SaveChanges();
            return comment;
        }

        // Создание файла
        protected AccountingEmployeesActivities.Models.File CreateTestFile(AppTask task, string name = "file.txt", byte[] data = null)
        {
            if (data == null) data = new byte[] { 0x01, 0x02, 0x03 };
            var file = new AccountingEmployeesActivities.Models.File
            {
                IdTask = task.IdTask,
                Name = name,
                AddDate = DateOnly.FromDateTime(DateTime.Now),
                Data = data
            };
            Context.Files.Add(file);
            Context.SaveChanges();
            return file;
        }
    }
}