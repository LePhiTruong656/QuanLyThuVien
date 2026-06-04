using System;
using System.Configuration;
using System.IO;
using System.Text.Json;

namespace LibraryManagementFE.Services
{
    /// <summary>
    /// Class để lưu trữ preferences của user (Remember Me)
    /// </summary>
    public class UserPreferences
    {
        public bool RememberMe { get; set; }
        public string SavedEmail { get; set; } = string.Empty;
        public string EncryptedPassword { get; set; } = string.Empty;

        private static readonly string _preferencesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LibraryManagementUIT",
            "preferences.json");

        /// <summary>
        /// Load preferences từ file
        /// </summary>
        public static UserPreferences Load()
        {
            try
            {
                if (File.Exists(_preferencesPath))
                {
                    string json = File.ReadAllText(_preferencesPath);
                    return JsonSerializer.Deserialize<UserPreferences>(json) ?? new UserPreferences();
                }
            }
            catch
            {
                // Nếu có lỗi thì trả về preferences mới
            }

            return new UserPreferences();
        }

        /// <summary>
        /// Lưu preferences vào file
        /// </summary>
        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(_preferencesPath)!;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_preferencesPath, json);
            }
            catch
            {
                // Nếu không save được thì bỏ qua
            }
        }

        /// <summary>
        /// Xóa preferences đã lưu
        /// </summary>
        public static void Clear()
        {
            try
            {
                if (File.Exists(_preferencesPath))
                {
                    File.Delete(_preferencesPath);
                }
            }
            catch
            {
                // Nếu không xóa được thì bỏ qua
            }
        }
    }
}
