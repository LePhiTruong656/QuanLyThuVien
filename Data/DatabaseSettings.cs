using System.IO;
using Microsoft.Extensions.Configuration;

namespace LibraryManagementFE.Data
{
    public static class DatabaseSettings
    {
        public const string ConnectionStringName = "LibraryDb";

        public static string GetConnectionString()
        {
            var fromEnv = Environment.GetEnvironmentVariable("LIBRARY_DB_CONNECTION");
            if (!string.IsNullOrWhiteSpace(fromEnv))
                return fromEnv;

            var basePath = ResolveConfigDirectoryForApp();
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            return config.GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException(
                    $"Connection string '{ConnectionStringName}' is missing. Set it in appsettings.json or LIBRARY_DB_CONNECTION.");
        }

        public static string ResolveConfigDirectoryForApp()
        {
            foreach (var candidate in new[]
            {
                AppContext.BaseDirectory,
                Directory.GetCurrentDirectory(),
            })
            {
                var dir = candidate;
                for (var i = 0; i < 6 && !string.IsNullOrEmpty(dir); i++)
                {
                    if (File.Exists(Path.Combine(dir, "appsettings.json")))
                        return dir;
                    dir = Directory.GetParent(dir)?.FullName ?? string.Empty;
                }
            }

            return AppContext.BaseDirectory;
        }
    }
}
