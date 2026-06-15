namespace LibraryManagementFE.Services
{
    public class OAuthSettings
    {
        public string GoogleClientId { get; set; } = string.Empty;
        public string GoogleClientSecret { get; set; } = string.Empty;
        public string FacebookAppId { get; set; } = string.Empty;
        public string FacebookAppSecret { get; set; } = string.Empty;
        public int RedirectPort { get; set; } = 7890;
        public string RedirectPath { get; set; } = "/oauth/callback";

        /// <summary>
        /// Bật đăng nhập social giả lập khi chưa có Client ID (chỉ dùng local).
        /// </summary>
        public bool DevMode { get; set; }

        public string RedirectUri => $"https://127.0.0.1:{RedirectPort}{RedirectPath}";

        public bool IsGoogleConfigured =>
            !string.IsNullOrWhiteSpace(GoogleClientId) &&
            !GoogleClientId.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase);

        public bool IsFacebookConfigured =>
            !string.IsNullOrWhiteSpace(FacebookAppId) &&
            !FacebookAppId.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(FacebookAppSecret) &&
            !FacebookAppSecret.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase);
    }
}
