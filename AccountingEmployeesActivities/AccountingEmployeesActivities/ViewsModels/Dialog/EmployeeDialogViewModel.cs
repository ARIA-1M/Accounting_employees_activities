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
        private string _errorMessage;
        private bool _isErrorVisible;


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
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsErrorVisible
        {
            get => _isErrorVisible;
            set => SetProperty(ref _isErrorVisible, value);
        }

        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public event EventHandler<EmployeeFormModel> EmployeeSaved;

        // Конструктор для создания нового сотрудника
        public EmployeeDialogViewModel(int currentUserId)
        {
            DialogTitle = "Создание сотрудника";
            IsNewEmployee = true;

            EmployeeForm = new EmployeeFormModel
            {
                IdBoss = currentUserId,
                IsActive = true,
                Password = string.Empty,
                IdGlpi = null  // ID GLPI изначально пустой
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

        // Конструктор для редактирования существующего сотрудника
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
                IsActive = employee.IsActive,
                IdBoss = currentUserId,
                IdGlpi = employee.IdUserNavigation?.IdGlpi  // ID GLPI из пользователя
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

            HideError();
            // Проверка: логин не пустой
            if (string.IsNullOrWhiteSpace(EmployeeForm.Login))
            {
                ShowError("Введите логин");
                return;
            }

            // Проверка: пароль не пустой (только при создании)
            if (IsNewEmployee && string.IsNullOrWhiteSpace(EmployeeForm.Password))
            {
                ShowError("Введите пароль");
                return;
            }

            // Проверка: фамилия не пустая
            if (string.IsNullOrWhiteSpace(EmployeeForm.LastName))
            {
                ShowError("Введите фамилию");
                return;
            }

            // Проверка: имя не пустое
            if (string.IsNullOrWhiteSpace(EmployeeForm.FirstName))
            {
                ShowError("Введите имя");
                return;
            }

            // Проверка на уникальность логина
            if (!IsLoginUnique(EmployeeForm.Login, IsNewEmployee ? null : EmployeeForm.IdEmployee))
            {
                ShowError("Пользователь с таким логином уже существует. Введите другой логин.");
                return;
            }

            EmployeeSaved?.Invoke(this, EmployeeForm);
        }

        // Проверка уникальности логина
        private bool IsLoginUnique(string login, int? excludeEmployeeId = null)
        {
            using var db = new PostgresContext();
            var query = db.Users.Where(u => u.Login == login);


            if (excludeEmployeeId.HasValue)
            {

                var employee = db.Employees
                    .FirstOrDefault(e => e.IdEmployee == excludeEmployeeId.Value);

                if (employee != null)
                {
                    query = query.Where(u => u.IdUser != employee.IdUser);
                }
            }
            return !query.Any();
        }

        private void Cancel()
        {
            EmployeeSaved?.Invoke(this, null);
        }

        private void ShowError(string message)
        {


            ErrorMessage = message;
            IsErrorVisible = true;
        }

        private void HideError()
        {
            ErrorMessage = string.Empty;
            IsErrorVisible = false;
        }
    }
}