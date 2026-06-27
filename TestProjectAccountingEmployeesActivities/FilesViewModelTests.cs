using AccountingEmployeesActivities.ViewModels;
using NUnit.Framework;
using System;
using System.Linq;
using AppTask = AccountingEmployeesActivities.Models.Task;
using ModelsFile = AccountingEmployeesActivities.Models.File;

namespace Accounting.Tests.Integration
{
    [TestFixture]
    [NonParallelizable]
    public class FilesViewModelTests : TestBase
    {
        [Test]
        public void Constructor_ShouldLoadExistingFiles()
        {
            var task = CreateTestTask();
            var file1 = CreateTestFile(task, "doc1.txt");
            var file2 = CreateTestFile(task, "doc2.pdf");

            var vm = new FilesViewModel(task.IdTask, Context);

            Assert.That(vm.Files, Has.Count.EqualTo(2));
            var names = vm.Files.Select(f => f.Name).ToArray();
            Assert.That(names, Is.EquivalentTo(new[] { "doc1.txt", "doc2.pdf" }));
        }

        [Test]
        public void Constructor_WhenNoFiles_ShouldReturnEmptyList()
        {
            var task = CreateTestTask();
            var vm = new FilesViewModel(task.IdTask, Context);
            Assert.That(vm.Files, Is.Empty);
        }

        [Test]
        public void FileProperties_ShouldBeCorrectlyMapped()
        {
            var task = CreateTestTask();
            var uploadDate = DateOnly.FromDateTime(DateTime.Now);
            var fileEntity = new ModelsFile
            {
                IdTask = task.IdTask,
                Name = "report.xlsx",
                Data = new byte[] { 0x01, 0x02 },
                AddDate = uploadDate
            };
            Context.Files.Add(fileEntity);
            Context.SaveChanges();

            var vm = new FilesViewModel(task.IdTask, Context);
            var file = vm.Files.First();

            Assert.That(file.Name, Is.EqualTo("report.xlsx"));
        }

        [Test]
        public void DownloadCommand_ShouldNotThrow_WhenFileExists()
        {
            var task = CreateTestTask();
            var file = CreateTestFile(task, "file.pdf");
            var vm = new FilesViewModel(task.IdTask, Context);
            var targetFile = vm.Files.First();

            Assert.DoesNotThrow(() => vm.DownloadFileCommand.Execute(targetFile));
        }

        [Test]
        public void DownloadCommand_WithNullParameter_ShouldNotThrow()
        {
            var task = CreateTestTask();
            var vm = new FilesViewModel(task.IdTask, Context);
            Assert.DoesNotThrow(() => vm.DownloadFileCommand.Execute(null));
        }


        [Test]
        public void TaskTitle_ShouldBeSetFromTask()
        {
            var task = CreateTestTask("My Special Task");
            var vm = new FilesViewModel(task.IdTask, Context);
            Assert.That(vm.TaskTitle, Is.EqualTo("My Special Task"));
        }
    }
}