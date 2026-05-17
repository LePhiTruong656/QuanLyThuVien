using System.IO;
using System.Text.Json;

namespace LibraryManagementFE.Policies
{
    public static class LibraryPolicyStore
    {
        private const string FileName = "library-policy.json";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public static string GetConfigPath()
        {
            // Prefer editable workspace path when running from source.
            var cwd = Directory.GetCurrentDirectory();
            var candidates = new[]
            {
                Path.Combine(cwd, "Policies", FileName),
                Path.Combine(cwd, FileName),
                Path.Combine(AppContext.BaseDirectory, FileName),
            };

            foreach (var p in candidates)
            {
                var dir = Path.GetDirectoryName(p);
                if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir) && File.Exists(p))
                    return p;
            }

            // Default create location.
            return Path.Combine(cwd, "Policies", FileName);
        }

        public static LibraryPolicy LoadOrCreate(string? path = null)
        {
            var p = path ?? GetConfigPath();

            var dir = Path.GetDirectoryName(p);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(p))
            {
                var defaults = LibraryPolicy.Default();
                Save(p, defaults);
                return defaults;
            }

            var json = File.ReadAllText(p);
            var policy = JsonSerializer.Deserialize<LibraryPolicy>(json, JsonOptions);
            return policy ?? LibraryPolicy.Default();
        }

        public static LibraryPolicy LoadOrCreate(out string configPath)
        {
            configPath = GetConfigPath();
            return LoadOrCreate(configPath);
        }

        public static void Save(string path, LibraryPolicy policy)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(policy, JsonOptions);
            File.WriteAllText(path, json);
        }
    }
}
