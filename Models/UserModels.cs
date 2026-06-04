using System.ComponentModel.DataAnnotations;

namespace LibraryManagementFE.Models
{
    public class User
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(512)]
        public string PasswordHash { get; set; } = string.Empty;

        // Liên kết với Reader (nullable vì có thể có user chưa hoàn thành đăng ký)
        [MaxLength(64)]
        public string? ReaderId { get; set; }

        public string CreatedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
