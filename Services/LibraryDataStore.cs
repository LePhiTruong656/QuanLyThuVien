using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using LibraryManagementFE.Models;

namespace LibraryManagementFE.Services
{
    public class LibraryDataStore
    {
        public List<ReaderRecord> Readers { get; set; } = new();
        public List<BookRecord> Books { get; set; } = new();
        public List<BorrowRecord> Borrows { get; set; } = new();
        public List<PaymentRecord> Payments { get; set; } = new();
    }

    public static class LibraryDataStoreFile
    {
        private const string FileName = "library-data.json";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public static string GetStorePath()
        {
            var cwd = Directory.GetCurrentDirectory();
            var dataFolder = Path.Combine(cwd, "Data");
            Directory.CreateDirectory(dataFolder);
            return Path.Combine(dataFolder, FileName);
        }

        public static LibraryDataStore LoadOrCreate()
        {
            var path = GetStorePath();
            if (!File.Exists(path))
            {
                var store = new LibraryDataStore();
                Save(store);
                return store;
            }

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<LibraryDataStore>(json, JsonOptions) ?? new LibraryDataStore();
        }

        public static void Save(LibraryDataStore store)
        {
            var path = GetStorePath();
            var json = JsonSerializer.Serialize(store, JsonOptions);
            File.WriteAllText(path, json);
        }
    }
}
