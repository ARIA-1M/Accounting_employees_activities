using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

// Настройки локального сохранения данных пользователя

namespace AccountingEmployeesActivities.Services
{
    public class SettingsService
    {
        private readonly string _settingsPath;
        private readonly EncryptionService _encryptionService;

        public SettingsService()
        {
            _encryptionService = new EncryptionService();

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "AccountingEmployeesActivities");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            _settingsPath = Path.Combine(appFolder, "settings.json");
        }

        public void SaveCredentials(string login, string password)
        {
            var settings = new UserSettings
            {
                Login = login,
                Password = _encryptionService.Encrypt(password)  
            };

            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(_settingsPath, json);
        }

        public UserSettings LoadCredentials()
        {
            if (!File.Exists(_settingsPath))
            {
                return new UserSettings { Login = "", Password = "" };
            }

            try
            {
                string json = File.ReadAllText(_settingsPath);
                var settings = JsonSerializer.Deserialize<UserSettings>(json);

                if (settings != null && !string.IsNullOrEmpty(settings.Password))
                {
                    settings.Password = _encryptionService.Decrypt(settings.Password); 
                }

                return settings ?? new UserSettings();
            }
            catch
            {
                return new UserSettings { Login = "", Password = "" };
            }
        }
    }

    public class UserSettings
    {
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
    }

}
