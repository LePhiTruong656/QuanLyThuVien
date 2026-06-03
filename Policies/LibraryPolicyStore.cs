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
            var policy = JsonSerializer.Deserialize<LibraryPolicy>(json, JsonOptions) ?? LibraryPolicy.Default();
            var defaultPolicy = LibraryPolicy.Default();
            policy.MinAge = policy.MinAge > 0 ? policy.MinAge : defaultPolicy.MinAge;
            policy.MaxAge = policy.MaxAge > 0 ? policy.MaxAge : defaultPolicy.MaxAge;
            policy.MaxBooksPerReader = policy.MaxBooksPerReader > 0 ? policy.MaxBooksPerReader : defaultPolicy.MaxBooksPerReader;
            policy.MaxLoanDays = policy.MaxLoanDays > 0 ? policy.MaxLoanDays : defaultPolicy.MaxLoanDays;
            policy.MaxRenewals = policy.MaxRenewals > 0 ? policy.MaxRenewals : defaultPolicy.MaxRenewals;
            policy.PenaltyPerDay = policy.PenaltyPerDay > 0 ? policy.PenaltyPerDay : defaultPolicy.PenaltyPerDay;
            return policy;
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
