using AccountingEmployeesActivities.Models;
using AccountingEmployeesActivities.ViewModels;
using Avalonia.Controls;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace AccountingEmployeesActivities.ViewsModels.Dialog
{
    public class DelegateTaskDialogViewModel : ViewModelBase
    {
        private readonly int _taskId;
        private readonly int _currentEmployeeId;
        private readonly Window _window;

        private ObservableCollection<Employee> _colleagues = new ObservableCollection<Employee>();
        private Employee _selectedColleague;
        private string _comment;
        private string _errorMessage;

        public ObservableCollection<Employee> Colleagues
        {
            get => _colleagues;
            set => SetProperty(ref _colleagues, value);
        }

        public Employee SelectedColleague
        {
            get => _selectedColleague;
            set => SetProperty(ref _selectedColleague, value);
        }

        public string Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public DelegateTaskDialogViewModel(int taskId, int currentEmployeeId, Window window)
        {
            _taskId = taskId;
            _currentEmployeeId = currentEmployeeId;
            _window = window;

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => _window.Close());

            LoadColleagues();
        }

        private void LoadColleagues()
        {
            using var db = new PostgresContext();


            var currentEmployee = db.Employees.FirstOrDefault(e => e.IdEmployee == _currentEmployeeId);
            if (currentEmployee == null)
            {
                ErrorMessage = "Текущий сотрудник не найден.";
                return;
            }
            if (currentEmployee.IdBoss == null)
            {
                ErrorMessage = "У текущего сотрудника не указан руководитель.";
                Colleagues = new ObservableCollection<Employee>();
                return;
            }
            var colleagues = db.Employees
                .Where(e => e.IdBoss == currentEmployee.IdBoss
                            && e.IdEmployee != _currentEmployeeId
                            && e.IsActive == true)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToList();
            System.Diagnostics.Debug.WriteLine($"Colleagues count = {colleagues.Count}");
            foreach (var c in colleagues)
            {
                System.Diagnostics.Debug.WriteLine($"Colleague: {c.LastName} {c.FirstName}, IdBoss={c.IdBoss}");
            }

            Colleagues = new ObservableCollection<Employee>(colleagues);
        }

        private void Save()
        {
            ErrorMessage = string.Empty;

            if (SelectedColleague == null)
            {
                ErrorMessage = "Выберите сотрудника.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Comment))
            {
                ErrorMessage = "Введите комментарий причины делегирования.";
                return;
            }

            using var db = new PostgresContext();

            var currentEmployee = db.Employees.FirstOrDefault(e => e.IdEmployee == _currentEmployeeId);
            var selectedEmployee = db.Employees.FirstOrDefault(e => e.IdEmployee == SelectedColleague.IdEmployee);

            if (currentEmployee == null || selectedEmployee == null)
            {
                ErrorMessage = "Ошибка поиска сотрудников.";
                return;
            }

            // защита: можно делегировать только коллеге с тем же руководителем
            if (currentEmployee.IdBoss != selectedEmployee.IdBoss)
            {
                ErrorMessage = "Можно делегировать задачу только коллеге вашего отдела.";
                return;
            }

            var activeExecutor = db.Executors
                .FirstOrDefault(e => e.IdTask == _taskId
                                  && e.IdEmployee == _currentEmployeeId
                                  && e.IsActive == true);

            if (activeExecutor == null)
            {
                ErrorMessage = "Активный исполнитель по задаче не найден.";
                return;
            }

            // деактивируем старую запись
            activeExecutor.IsActive = false;
            activeExecutor.ChangeDate = DateOnly.FromDateTime(DateTime.Today);

            // создаем новую запись
            var newExecutor = new Executor
            {
                IdTask = _taskId,
                IdEmployee = selectedEmployee.IdEmployee,
                IsActive = true,
                Comment = Comment.Trim(),
                ChangeDate = DateOnly.FromDateTime(DateTime.Today)
            };

            db.Executors.Add(newExecutor);

            // переводим задачу в статус "Делегирование"
            var task = db.Tasks.FirstOrDefault(t => t.IdTask == _taskId);
            if (task != null)
            {
                task.IdStatus = 5;
            }

            db.SaveChanges();
            _window.Close();
        }
    }

}
