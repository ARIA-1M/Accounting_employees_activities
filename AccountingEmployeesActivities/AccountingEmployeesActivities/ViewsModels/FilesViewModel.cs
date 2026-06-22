using AccountingEmployeesActivities.Models;
using Avalonia.Platform.Storage;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AccountingEmployeesActivities.ViewModels
{
    public class FilesViewModel : ViewModelBase
    {
        private readonly int _taskId;
        private string _taskTitle = string.Empty;
        public ObservableCollection<FileItem> Files { get; }
        public ICommand DownloadFileCommand { get; }
        public string TaskTitle
        {
            get => _taskTitle;
            set => SetProperty(ref _taskTitle, value);
        }

        public FilesViewModel(int taskId)
        {
            _taskId = taskId;
            Files = new ObservableCollection<FileItem>();
            DownloadFileCommand = new RelayCommand(DownloadFile);
            LoadFiles();
        }

        private void LoadFiles()
        {
            using var db = new PostgresContext();
            var task = db.Tasks.FirstOrDefault(t => t.IdTask == _taskId);
            TaskTitle = task?.Name ?? "Задача";

            var fileEntities = db.Files.Where(f => f.IdTask == _taskId).ToList();
            foreach (var file in fileEntities)
            {
                Files.Add(new FileItem
                {
                    IdFile = file.IdFile,
                    Name = file.Name,
                    Data = file.Data ?? Array.Empty<byte>()
                });
            }
        }

        private async void DownloadFile(object parameter)
        {
            // Проверяем, что параметр - FileItem
            if (parameter is not FileItem fileItem)
                return;

            if (fileItem.Data == null || fileItem.Data.Length == 0)
            {
                var tempData = System.Text.Encoding.UTF8.GetBytes($"Файл {fileItem.Name} (содержимое отсутствует)");
                await SaveFile(fileItem.Name, tempData);
                return;
            }

            await SaveFile(fileItem.Name, fileItem.Data);
        }

        private async System.Threading.Tasks.Task SaveFile(string fileName, byte[] data)
        {
            var storageProvider = Avalonia.Application.Current?.ApplicationLifetime?
                .GetType().GetProperty("StorageProvider")?.GetValue(null) as IStorageProvider;
            if (storageProvider == null) return;

            var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Сохранить файл",
                SuggestedFileName = fileName
            });

            if (file != null)
            {
                await using var stream = await file.OpenWriteAsync();
                await stream.WriteAsync(data, 0, data.Length);
            }
        }

        protected void SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }
    }

    public class FileItem
    {
        public int IdFile { get; set; }
        public string Name { get; set; }
        public byte[] Data { get; set; }
    }
}