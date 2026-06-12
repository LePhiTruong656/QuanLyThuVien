using LibraryManagementFE.Data;
using Microsoft.Extensions.Configuration;

namespace LibraryManagementFE.Services
{
    public static class AppConfiguration
    {
        private static IConfiguration? _configuration;

        private static IConfiguration Configuration => _configuration ??= BuildConfiguration();

        public static OAuthSettings GetOAuthSettings()
        {
            var section = Configuration.GetSection("OAuth");
            return new OAuthSettings
            {
                GoogleClientId = section["Google:ClientId"] ?? string.Empty,
                GoogleClientSecret = section["Google:ClientSecret"] ?? string.Empty,
                FacebookAppId = section["Facebook:AppId"] ?? string.Empty,
                FacebookAppSecret = section["Facebook:AppSecret"] ?? string.Empty,
                RedirectPort = int.TryParse(section["RedirectPort"], out var port) ? port : 7890,
                RedirectPath = section["RedirectPath"] ?? "/oauth/callback",
                DevMode = bool.TryParse(section["DevMode"], out var devMode) && devMode
            };
        }

        private static IConfiguration BuildConfiguration()
        {
            var basePath = DatabaseSettings.ResolveConfigDirectoryForApp();
            return new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .Build();
        }
    }
}
