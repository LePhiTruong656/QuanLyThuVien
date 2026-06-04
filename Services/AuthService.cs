using BCrypt.Net;
using LibraryManagementFE.Data;
using LibraryManagementFE.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace LibraryManagementFE.Services
{
    public class AuthService
    {
        private readonly LibraryDbContext _context;

        public AuthService(LibraryDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Hash password sử dụng BCrypt
        /// </summary>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Xác thực password với hash đã lưu
        /// </summary>
        public bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        /// <summary>
        /// Kiểm tra email đã tồn tại chưa
        /// </summary>
        public bool EmailExists(string email)
        {
            return _context.Users.Any(u => u.Email.ToLower() == email.ToLower());
        }

        /// <summary>
        /// Đăng ký user mới
        /// </summary>
        public (bool success, string message, User? user) Register(string email, string password, string? readerId = null)
        {
            try
            {
                // Kiểm tra email đã tồn tại
                if (EmailExists(email))
                {
                    return (false, "Email đã được sử dụng.", null);
                }

                // Tạo user mới
                var user = new User
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Email = email.ToLower().Trim(),
                    PasswordHash = HashPassword(password),
                    ReaderId = readerId,
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                return (true, "Đăng ký thành công!", user);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        public (bool success, string message, User? user) Login(string email, string password)
        {
            try
            {
                var user = _context.Users
                    .FirstOrDefault(u => u.Email.ToLower() == email.ToLower().Trim());

                if (user == null)
                {
                    return (false, "Email không tồn tại.", null);
                }

                if (!VerifyPassword(password, user.PasswordHash))
                {
                    return (false, "Mật khẩu không chính xác.", null);
                }

                return (true, "Đăng nhập thành công!", user);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Lấy thông tin reader từ user
        /// </summary>
        public ReaderRecord? GetReaderByUserId(string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user?.ReaderId == null)
                return null;

            return _context.Readers.FirstOrDefault(r => r.Id == user.ReaderId);
        }

        /// <summary>
        /// Cập nhật ReaderId cho user (sau khi hoàn tất đăng ký)
        /// </summary>
        public bool LinkReaderToUser(string userId, string readerId)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                    return false;

                user.ReaderId = readerId;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
