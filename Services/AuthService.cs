using BCrypt.Net;
using LibraryManagementFE.Data;
using LibraryManagementFE.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementFE.Services
{
    public class AuthService
    {
        private readonly LibraryDbContext _context;

        public AuthService(LibraryDbContext context)
        {
            _context = context;
        }

        public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

        public bool VerifyPassword(string password, string passwordHash) =>
            BCrypt.Net.BCrypt.Verify(password, passwordHash);

        public bool EmailExists(string email) =>
            _context.Users.Any(u => u.Email.ToLower() == email.ToLower());

        public (bool success, string message, User? user) Register(string email, string password, string? readerId = null)
        {
            try
            {
                if (EmailExists(email))
                    return (false, "Email đã được sử dụng.", null);

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

        public (bool success, string message, User? user) Login(string email, string password)
        {
            try
            {
                var user = _context.Users
                    .FirstOrDefault(u => u.Email.ToLower() == email.ToLower().Trim());

                if (user == null)
                    return (false, "Email không tồn tại.", null);

                if (VerifyPassword(password, user.PasswordHash))
                    return (true, "Đăng nhập thành công!", user);

                if (IsSocialOnlyAccount(user))
                {
                    return (false,
                        "Tài khoản này đăng ký qua Google hoặc Facebook. Vui lòng đăng nhập bằng các nút tương ứng.",
                        null);
                }

                return (false, "Mật khẩu không chính xác.", null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}", null);
            }
        }

        public (bool success, string message, User? user) LoginOrRegisterSocial(SocialLoginProfile profile)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(profile.Email))
                    return (false, "Thông tin tài khoản mạng xã hội không hợp lệ.", null);

                var email = profile.Email.ToLower().Trim();
                var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email);

                if (user != null)
                    return (true, "Đăng nhập thành công!", user);

                user = new User
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Email = email,
                    PasswordHash = HashPassword(SocialAccountHelper.PasswordSeed(email)),
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                _context.Users.Add(user);
                _context.SaveChanges();
                return (true, "Đăng nhập thành công!", user);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}", null);
            }
        }

        private static bool IsSocialOnlyAccount(User user) =>
            BCrypt.Net.BCrypt.Verify(SocialAccountHelper.PasswordSeed(user.Email), user.PasswordHash);

        public ReaderRecord? GetReaderByUserId(string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user?.ReaderId == null)
                return null;

            return _context.Readers.FirstOrDefault(r => r.Id == user.ReaderId);
        }

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
