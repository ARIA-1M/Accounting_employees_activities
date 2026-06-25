using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

// Настройки локального сохранения данных пользователя

namespace AccountingEmployeesActivities.Services
{
    public class StatisticGLPIService
    {
        private readonly string _statisticGLPIPath;
        private readonly EncryptionService _encryptionService;

        public StatisticGLPIService()
        {
            _encryptionService = new EncryptionService();

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "AccountingEmployeesActivities");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            _statisticGLPIPath = Path.Combine(appFolder, "statisticGLPI.json");
        }

        public void SaveCredentials(string login, string password)
        {
            var statisticGLPI = new UserSettings
            {
                Login = login,
                Password = _encryptionService.Encrypt(password)  
            };

            string json = JsonSerializer.Serialize(statisticGLPI);
            File.WriteAllText(_statisticGLPIPath, json);
        }

        public UserSettings LoadCredentials()
        {
            if (!File.Exists(_statisticGLPIPath))
            {
                return new UserSettings { Login = "", Password = "" };
            }

            try
            {
                string json = File.ReadAllText(_statisticGLPIPath);
                var statisticGLPI = JsonSerializer.Deserialize<UserSettings>(json);

                if (statisticGLPI != null && !string.IsNullOrEmpty(statisticGLPI.Password))
                {
                    statisticGLPI.Password = _encryptionService.Decrypt(statisticGLPI.Password); 
                }

                return statisticGLPI ?? new UserSettings();
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
