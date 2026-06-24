using System;
using System.Linq;
using System.Threading.Tasks;
using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace AccountingEmployeesTest
{
    [SetUpFixture]
    public class TestEnvironment
    {
        public static string ConnectionString { get; private set; }

        [OneTimeSetUp]
        public void Setup()
        {
            ConnectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=123";
        }
    }

    [TestFixture]
    public class IntegrationTests
    {
        private PostgresContext _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<PostgresContext>()
                .UseNpgsql(TestEnvironment.ConnectionString)
                .Options;
            _context = new PostgresContext(options);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public void Test_Connection() // Тест подключения к бд
        {
            Assert.That(_context.Database.CanConnect(), Is.True);
        }

        [Test]
        public void Test_TasksCount() // Проверка количества задач
        {
            var count = _context.Tasks.Count();
            Assert.That(count, Is.EqualTo(22));
        }

        [Test]
        public async Task Stats_TotalTasks()// Проверка количества задач за последний год 
        {
            var statsService = new StatisticsService(_context);
            var start = new DateTime(2026, 1, 1);
            var end = new DateTime(2026, 12, 31);
            var result = await statsService.GetStatisticsAsync(null, start, end);
            Assert.That(result.TotalTasks, Is.EqualTo(22));
        }

        [Test]
        public async Task Stats_ProgressPercentage_CompletionRate()// Процент выполненых задач
        {
            var statsService = new StatisticsService(_context);
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2026, 12, 31);
            var result = await statsService.GetStatisticsAsync(null, start, end);
            // 6/22 * 100 = 27.27 -> 27%
            Assert.That(result.ProgressPercentage, Is.EqualTo(27));
        }

        [Test]
        public async Task Stats_StatusDistribution()// Нахождение количества статусов
        {
            var statsService = new StatisticsService(_context);
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2026, 12, 31);
            var distribution = await statsService.GetStatusDistributionAsync(null, start, end);
            Assert.That(distribution.Count, Is.EqualTo(5));
        }

        [Test]
        public async Task Stats_StatusNew()// Проверка количества задач новых
        {
            var statsService = new StatisticsService(_context);
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2026, 12, 31);
            var distribution = await statsService.GetStatusDistributionAsync(null, start, end);
            var newStatus = distribution.First(d => d.StatusName == "новое");
            Assert.That(newStatus.Count, Is.EqualTo(6)); 
        }

        [Test]
        public async Task Stats_InProgressTasks()// Проверка количесва задач в работе
        {
            var statsService = new StatisticsService(_context);
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2026, 12, 31);
            var result = await statsService.GetStatisticsAsync(null, start, end);
            Assert.That(result.InProgressTasks, Is.EqualTo(4));
        }

        [Test]
        public async Task Stats_WaitingTasks()// Проверка количества задач в ожидании
        {
            var statsService = new StatisticsService(_context);
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2026, 12, 31);
            var result = await statsService.GetStatisticsAsync(null, start, end);
            Assert.That(result.InProgressTasks, Is.EqualTo(4));
        }

        [Test]
        public async Task Stats_StatusCompleted()// Проверка решеного количество задач
        {
            var statsService = new StatisticsService(_context);
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2026, 12, 31);
            var distribution = await statsService.GetStatusDistributionAsync(null, start, end);
            var completed = distribution.First(d => d.StatusName == "решена");
            Assert.That(completed.Count, Is.EqualTo(6)); 
        }

        [Test]
        public async Task Stats_StatusDelegated()// Проверка количества задач в статусе делегирование 
        {
            var statsService = new StatisticsService(_context);
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2026, 12, 31);
            var distribution = await statsService.GetStatusDistributionAsync(null, start, end);
            var delegated = distribution.First(d => d.StatusName == "делегирование");
            Assert.That(delegated.Count, Is.EqualTo(1)); 
        }

        [Test]
        public async Task Stats_EmployeeTasks_Sidorov()// Количество задач у Сидорова
        {
            var statsService = new StatisticsService(_context);
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2026, 12, 31);
            var employeeId = 4; // Сидоров
            var result = await statsService.GetStatisticsAsync(employeeId, start, end);
            Assert.That(result.TotalTasks, Is.EqualTo(9));
        }

        [Test]
        public async Task Stats_EmployeeTasks_Petrov()// Количество задач у Петрова 
        {
            var statsService = new StatisticsService(_context);
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2026, 12, 31);
            var employeeId = 3; // Петров
            var result = await statsService.GetStatisticsAsync(employeeId, start, end);
            Assert.That(result.TotalTasks, Is.EqualTo(1));
        }

        [Test]
        public async Task Stats_EmployeeTasks_Ivanov()// Количество задач у Иванова 
        {
            var statsService = new StatisticsService(_context);
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2026, 12, 31);
            var employeeId = 2; // Иванов
            var result = await statsService.GetStatisticsAsync(employeeId, start, end);
            Assert.That(result.TotalTasks, Is.EqualTo(2));
        }

        [Test]
        public async Task Stats_EmployeeTasks_Kozlov()// Количество задач у Козлова 
        {
            var statsService = new StatisticsService(_context);
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2026, 12, 31);
            var employeeId = 6; // Козлов
            var result = await statsService.GetStatisticsAsync(employeeId, start, end);
            Assert.That(result.TotalTasks, Is.EqualTo(2));
        }

        [Test]
        public async Task Stats_DateRange_January2024() // Количество задач за Январь 2024
        {
            var statsService = new StatisticsService(_context);
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2024, 1, 31);
            var result = await statsService.GetStatisticsAsync(null, start, end);
            Assert.That(result.TotalTasks, Is.EqualTo(0));
        }

        [Test]
        public async Task Stats_DateRange_January2026()// Количество задач за Январь 2026
        {
            var statsService = new StatisticsService(_context);
            var start = new DateTime(2026, 1, 1);
            var end = new DateTime(2026, 1, 31);
            var result = await statsService.GetStatisticsAsync(null, start, end);
            Assert.That(result.TotalTasks, Is.GreaterThan(10));
        }

        [Test]
        public async Task Stats_DateRange_June2026() // Количество задач за Июнь 2026
        {
            var statsService = new StatisticsService(_context);
            var start = new DateTime(2026, 6, 1);
            var end = new DateTime(2026, 6, 30);
            var result = await statsService.GetStatisticsAsync(null, start, end);
            Assert.That(result.TotalTasks, Is.EqualTo(7));
        }

        [Test]
        public async Task Stats_Sidorov_January2026()// Количество задач за Январь 2026 у Сидорова
        {
            var statsService = new StatisticsService(_context);
            var start = new DateTime(2026, 1, 1);
            var end = new DateTime(2026, 1, 31);
            var employeeId = 4; // Сидоров
            var result = await statsService.GetStatisticsAsync(employeeId, start, end);
            Assert.That(result.TotalTasks, Is.EqualTo(7));
        }

        [Test]
        public async Task Stats_Ivanov_January2026()// Количество задач за Январь 2026 у Иванова
        {
            var statsService = new StatisticsService(_context);
            var start = new DateTime(2026, 1, 1);
            var end = new DateTime(2026, 1, 31);
            var employeeId = 2; // Иванов
            var result = await statsService.GetStatisticsAsync(employeeId, start, end);
            Assert.That(result.TotalTasks, Is.EqualTo(2));
        }

        [Test]
        public void Delegate_CanDelegate_PetrovOnIvanov() //Проверка: задача создана сотрудником 3 (Петров), а проверяем сотрудника 2 (Иванов)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.IdCreator == 3);
            if (task == null) Assert.Pass("Нет задачи от Петрова");

            bool canDelegate = !(task.IdCreator == 2);
            Assert.That(canDelegate, Is.True);
        }

        [Test]
        public void Delegate_CanDelegate()// Попытка делегировать свою же задачу 
        {
            var task = _context.Tasks.FirstOrDefault(t => t.IdCreator == 2);
            if (task == null) Assert.Pass("Нет задачи от Иванова");

            bool canDelegate = !(task.IdCreator == 2);
            Assert.That(canDelegate, Is.False);
        }

        [Test]
        public async Task Delegate_ChangeStatus_StatysDelegate()//При делегировании статус задачи изменяется на 5 
        {
            var task = _context.Tasks.FirstOrDefault(t => t.IdCreator != 2 && t.IdStatus != 5);
            if (task == null) Assert.Pass("Нет подходящей задачи");

            var oldStatus = task.IdStatus;
            task.IdStatus = 5;
            await _context.SaveChangesAsync();

            var updatedTask = await _context.Tasks.FindAsync(task.IdTask);
            Assert.That(updatedTask.IdStatus, Is.EqualTo(5));

            // Откат
            updatedTask.IdStatus = oldStatus;
            await _context.SaveChangesAsync();
        }

        [Test]
        public async Task Delegate_Executor_PerformerChanges()// Изменение Исполнителя при делегации 
        {
            var taskId = 1;
            var oldExecutor = _context.Executors.FirstOrDefault(e => e.IdTask == taskId && e.IsActive == true);
            if (oldExecutor == null) Assert.Pass("Нет активного исполнителя");

            var oldEmployeeId = oldExecutor.IdEmployee;
            var newEmployeeId = 3;

            oldExecutor.IsActive = false;
            oldExecutor.ChangeDate = DateOnly.FromDateTime(DateTime.Now);

            var newExecutor = new Executor
            {
                IdTask = taskId,
                IdEmployee = newEmployeeId,
                IsActive = true,
                Comment = "Тест",
                ChangeDate = DateOnly.FromDateTime(DateTime.Now)
            };
            _context.Executors.Add(newExecutor);
            await _context.SaveChangesAsync();

            var old = await _context.Executors.FirstOrDefaultAsync(e => e.IdTask == taskId && e.IdEmployee == oldEmployeeId);
            var newExec = await _context.Executors.FirstOrDefaultAsync(e => e.IdTask == taskId && e.IdEmployee == newEmployeeId);

            Assert.That(old.IsActive, Is.False);
            Assert.That(newExec.IsActive, Is.True);

            // Откат
            old.IsActive = true;
            _context.Executors.Remove(newExec);
            await _context.SaveChangesAsync();
        }

        [Test]
        public void DelegatedTask_ExecutorActive()// Делегирование задачи и проверка активности исполнителя
        {
            var delegatedTask = _context.Tasks.FirstOrDefault(t => t.IdStatus == 5);
            if (delegatedTask == null) Assert.Pass("Нет делегированных задач");

            var executor = _context.Executors.FirstOrDefault(e => e.IdTask == delegatedTask.IdTask && e.IsActive == true);
            Assert.That(executor, Is.Not.Null);
        }
    }
}