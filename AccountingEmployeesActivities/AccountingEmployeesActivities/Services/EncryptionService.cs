using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

// Шифровние пароля для сохранения на локальном устройстве

namespace AccountingEmployeesActivities.Services
{
    public class EncryptionService
    {
        //Шифрование
        public string Encrypt(string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }

        //Расшифровка
        public string Decrypt(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
