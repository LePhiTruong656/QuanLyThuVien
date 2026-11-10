namespace LibraryManagementFE.Models
{
    public class SocialLoginProfile
    {
        public string Provider { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
    }
}
