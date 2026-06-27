using AccountingEmployeesActivities.Models;
using Avalonia.Platform.Storage;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AccountingEmployeesActivities.ViewModels
{
    public class FilesViewModel : ViewModelBase
    {
        private readonly int _taskId;
        private string _taskTitle = string.Empty;
        private IStorageProvider _storageProvider;

        public ObservableCollection<FileItem> Files { get; }
        public ICommand DownloadFileCommand { get; }
        public ICommand AddFileCommand { get; }
        public string TaskTitle
        {
            get => _taskTitle;
            set => SetProperty(ref _taskTitle, value);
        }

        public void SetStorageProvider(IStorageProvider provider)
        {
            _storageProvider = provider;
        }
        public FilesViewModel(int taskId, PostgresContext db)
        {
            _taskId = taskId;
            Files = new ObservableCollection<FileItem>();
            DownloadFileCommand = new RelayCommand(DownloadFile);
            AddFileCommand = new RelayCommand(_ => AddFileAsync());
            LoadFiles(db);
        }

        public FilesViewModel(int taskId)
        {
            _taskId = taskId;
            Files = new ObservableCollection<FileItem>();
            DownloadFileCommand = new RelayCommand(DownloadFile);
            AddFileCommand = new RelayCommand(_ => AddFileAsync());
            using var db = new PostgresContext();            
            LoadFiles(db);
        }

        private void LoadFiles(PostgresContext db)
        {

            var task = db.Tasks.FirstOrDefault(t => t.IdTask == _taskId);
            TaskTitle = task?.Name ?? "Задача";

            var fileEntities = db.Files.Where(f => f.IdTask == _taskId).ToList();
            Files.Clear();
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
            if (parameter is not FileItem fileItem) return;
            if (_storageProvider == null) return;

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
            if (_storageProvider == null) return;
            var file = await _storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
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

        private async void AddFileAsync()
        {
            if (_storageProvider == null) return;

            var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Выберите файл для добавления",
                AllowMultiple = false
            });

            if (files == null || files.Count == 0) return;

            var selectedFile = files[0];
            var fileName = selectedFile.Name;
            byte[] fileData;

            await using (var stream = await selectedFile.OpenReadAsync())
            {
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                fileData = memoryStream.ToArray();
            }

            using var db = new PostgresContext();
            int maxId = db.Files.Any() ? db.Files.Max(f => f.IdFile) : 0;

            // Явно указываем пространство имён для модели File
            var newFile = new AccountingEmployeesActivities.Models.File
            {
                IdFile = maxId + 1,
                IdTask = _taskId,
                Name = fileName,
                AddDate = DateOnly.FromDateTime(DateTime.Now),
                Data = fileData
            };
            db.Files.Add(newFile);
            await db.SaveChangesAsync();

            Files.Add(new FileItem
            {
                IdFile = newFile.IdFile,
                Name = newFile.Name,
                Data = newFile.Data
            });
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