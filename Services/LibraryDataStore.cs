using System.IO;
using LibraryManagementFE.Data;
using LibraryManagementFE.Models;
using Microsoft.EntityFrameworkCore;

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
        public static LibraryDataStore LoadOrCreate()
        {
            using var db = CreateContext();
            db.Database.Migrate();

            return new LibraryDataStore
            {
                Readers = db.Readers.AsNoTracking().ToList(),
                Books = db.Books.AsNoTracking().ToList(),
                Borrows = db.Borrows.AsNoTracking().ToList(),
                Payments = db.Payments.AsNoTracking().ToList()
            };
        }

        public static void Save(LibraryDataStore store)
        {
            using var db = CreateContext();
            db.Database.Migrate();

            db.Payments.RemoveRange(db.Payments);
            db.Borrows.RemoveRange(db.Borrows);
            db.Books.RemoveRange(db.Books);
            db.Readers.RemoveRange(db.Readers);

            db.Readers.AddRange(store.Readers);
            db.Books.AddRange(store.Books);
            db.Borrows.AddRange(store.Borrows);
            db.Payments.AddRange(store.Payments);

            db.SaveChanges();
        }

        /// <summary>Legacy JSON path (kept for reference / optional import).</summary>
        public static string GetStorePath()
        {
            var cwd = Directory.GetCurrentDirectory();
            var dataFolder = Path.Combine(cwd, "Data");
            Directory.CreateDirectory(dataFolder);
            return Path.Combine(dataFolder, "library-data.json");
        }

        private static LibraryDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseSqlServer(DatabaseSettings.GetConnectionString())
                .Options;
            return new LibraryDbContext(options);
        }
    }
}
