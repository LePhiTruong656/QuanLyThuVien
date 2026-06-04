using System;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementFE.Services
{
    /// <summary>
    /// Helper class để mã hóa/giải mã credentials sử dụng DPAPI (Data Protection API)
    /// DPAPI chỉ cho phép user hiện tại trên máy này giải mã được
    /// </summary>
    public static class CredentialHelper
    {
        private static readonly byte[] _entropy = Encoding.UTF8.GetBytes("LibraryManagementUIT2026");

        /// <summary>
        /// Mã hóa plain text sử dụng DPAPI
        /// </summary>
        public static string Protect(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = ProtectedData.Protect(
                    plainBytes,
                    _entropy,
                    DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Giải mã encrypted text sử dụng DPAPI
        /// </summary>
        public static string Unprotect(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] plainBytes = ProtectedData.Unprotect(
                    encryptedBytes,
                    _entropy,
                    DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
