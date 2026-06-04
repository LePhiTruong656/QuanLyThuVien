using LibraryManagementFE.Models;

namespace LibraryManagementFE.Services
{
    /// <summary>
    /// Quản lý thông tin user hiện tại đã đăng nhập
    /// </summary>
    public static class CurrentUser
    {
        public static User? User { get; private set; }
        public static ReaderRecord? Reader { get; private set; }

        public static bool IsLoggedIn => User != null;

        /// <summary>
        /// Lưu thông tin user và reader sau khi đăng nhập thành công
        /// </summary>
        public static void SetUser(User user, ReaderRecord? reader = null)
        {
            User = user;
            Reader = reader;
        }

        /// <summary>
        /// Xóa thông tin user khi đăng xuất
        /// </summary>
        public static void Logout()
        {
            User = null;
            Reader = null;
        }

        /// <summary>
        /// Lấy tên hiển thị (tên reader hoặc email nếu chưa có reader)
        /// </summary>
        public static string GetDisplayName()
        {
            if (Reader != null && !string.IsNullOrEmpty(Reader.Name))
                return Reader.Name;

            if (User != null && !string.IsNullOrEmpty(User.Email))
            {
                // Lấy phần trước @ của email
                var emailParts = User.Email.Split('@');
                return emailParts.Length > 0 ? emailParts[0] : "User";
            }

            return "User";
        }
    }
}
