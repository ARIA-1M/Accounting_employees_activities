using AccountingEmployeesActivities.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace AccountingEmployeesActivities.ViewModels.Dialogs
{
    public class EmployeeDialogViewModel : ViewModelBase
    {
        private EmployeeFormModel _employeeForm;
        private ObservableCollection<Role> _roles;
        private string _dialogTitle;
        private bool _isNewEmployee;
        private string _employeeIdInfo;
        private Role _selectedRole;

        public EmployeeFormModel EmployeeForm
        {
            get => _employeeForm;
            set => SetProperty(ref _employeeForm, value);
        }

        public ObservableCollection<Role> Roles
        {
            get => _roles;
            set => SetProperty(ref _roles, value);
        }

        public string DialogTitle
        {
            get => _dialogTitle;
            set => SetProperty(ref _dialogTitle, value);
        }

        public bool IsNewEmployee
        {
            get => _isNewEmployee;
            set => SetProperty(ref _isNewEmployee, value);
        }

        public string EmployeeIdInfo
        {
            get => _employeeIdInfo;
            set => SetProperty(ref _employeeIdInfo, value);
        }

        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public event EventHandler<EmployeeFormModel> EmployeeSaved;

        public EmployeeDialogViewModel(int currentUserId)
        {
            DialogTitle = "Создание сотрудника";
            IsNewEmployee = true;

            EmployeeForm = new EmployeeFormModel
            {
                IdBoss = currentUserId,
                IsActive = true,
                Password = string.Empty
            };

            LoadRoles();
            InitCommands();
        }

        public Role SelectedRole
        {
            get => _selectedRole;
            set
            {
                SetProperty(ref _selectedRole, value);
                if (value != null)
                {
                    EmployeeForm.IdRole = value.IdRole;
                }
            }
        }

        public EmployeeDialogViewModel(Employee employee, int currentUserId)
        {
            DialogTitle = "Редактирование сотрудника";
            IsNewEmployee = false;

            LoadRoles();

            EmployeeForm = new EmployeeFormModel
            {
                IdEmployee = employee.IdEmployee,
                Login = employee.IdUserNavigation?.Login ?? string.Empty,
                Password = string.Empty,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                MiddleName = employee.MiddleName,
                IdRole = employee.IdUserNavigation?.IdRole ?? 3,
                IsActive = employee.IsActive.GetValueOrDefault(true),
                IdBoss = currentUserId
            };

            EmployeeIdInfo = $"ID сотрудника: {employee.IdEmployee}";

            InitCommands();
        }


        private void InitCommands()
        {
          
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private void LoadRoles()
        {
            using var db = new PostgresContext();
            Roles = new ObservableCollection<Role>(db.Roles.ToList());
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(EmployeeForm.Login))
            {
                ShowError("Введите логин");
                return;
            }

            if (IsNewEmployee && string.IsNullOrWhiteSpace(EmployeeForm.Password))
            {
                ShowError("Введите пароль");
                return;
            }

            if (string.IsNullOrWhiteSpace(EmployeeForm.LastName))
            {
                ShowError("Введите фамилию");
                return;
            }

            if (string.IsNullOrWhiteSpace(EmployeeForm.FirstName))
            {
                ShowError("Введите имя");
                return;
            }

            

            EmployeeSaved?.Invoke(this, EmployeeForm);
        }

        private void Cancel()
        {
            EmployeeSaved?.Invoke(this, null);
        }

        private void ShowError(string message)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка: {message}");
        }
    }
}