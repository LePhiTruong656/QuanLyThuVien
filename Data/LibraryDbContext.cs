using LibraryManagementFE.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementFE.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<ReaderRecord> Readers => Set<ReaderRecord>();
        public DbSet<BookRecord> Books => Set<BookRecord>();
        public DbSet<BorrowRecord> Borrows => Set<BorrowRecord>();
        public DbSet<PaymentRecord> Payments => Set<PaymentRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(64);
                entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
                entity.Property(e => e.PasswordHash).HasMaxLength(512).IsRequired();
                entity.Property(e => e.ReaderId).HasMaxLength(64);
                entity.Property(e => e.CreatedAt).HasMaxLength(32);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<ReaderRecord>(entity =>
            {
                entity.ToTable("Readers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(64);
                entity.Property(e => e.Name).HasMaxLength(256);
                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.CardNumber).HasMaxLength(64);
                entity.Property(e => e.DateOfBirth).HasMaxLength(32);
                entity.Property(e => e.RegDate).HasMaxLength(32);
                entity.Property(e => e.CardType).HasConversion<string>().HasMaxLength(32);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(32);
                entity.Ignore(e => e.Initials);
                entity.Ignore(e => e.AvatarBackground);
                entity.Ignore(e => e.AvatarForeground);
                entity.Ignore(e => e.AvatarBorder);
                entity.Ignore(e => e.CardTypeText);
                entity.Ignore(e => e.CardTypeBg);
                entity.Ignore(e => e.CardTypeFg);
                entity.Ignore(e => e.StatusText);
                entity.Ignore(e => e.StatusDotColor);
                entity.Ignore(e => e.StatusTextColor);
                entity.Ignore(e => e.StatusBg);
            });

            modelBuilder.Entity<BookRecord>(entity =>
            {
                entity.ToTable("Books");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(64);
                entity.Property(e => e.Title).HasMaxLength(512);
                entity.Property(e => e.Author).HasMaxLength(256);
                entity.Property(e => e.CategoryLine1).HasMaxLength(128);
                entity.Property(e => e.CategoryLine2).HasMaxLength(128);
                entity.Property(e => e.CoverImagePath).HasMaxLength(1024);
                entity.Property(e => e.CategoryPillBg).HasMaxLength(16);
                entity.Property(e => e.CategoryPillFg).HasMaxLength(16);
                entity.Property(e => e.Availability).HasConversion<string>().HasMaxLength(32);
                entity.Ignore(e => e.StatusText);
                entity.Ignore(e => e.StatusDotColor);
                entity.Ignore(e => e.StatusTextColor);
                entity.Ignore(e => e.CoverInitials);
                entity.Ignore(e => e.HasCoverImage);
            });

            modelBuilder.Entity<BorrowRecord>(entity =>
            {
                entity.ToTable("Borrows");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(64);
                entity.Property(e => e.MaPhieu).HasMaxLength(64);
                entity.Property(e => e.ReaderId).HasMaxLength(64);
                entity.Property(e => e.BookId).HasMaxLength(64);
                entity.Property(e => e.ReaderName).HasMaxLength(256);
                entity.Property(e => e.ReaderInitials).HasMaxLength(16);
                entity.Property(e => e.ReaderEmail).HasMaxLength(256);
                entity.Property(e => e.CardNumber).HasMaxLength(64);
                entity.Property(e => e.BookTitle).HasMaxLength(512);
                entity.Property(e => e.Author).HasMaxLength(256);
                entity.Property(e => e.ReturnNote).HasMaxLength(512);
                entity.Property(e => e.FinePaidDate).HasMaxLength(32);
                entity.Property(e => e.BorrowDate).HasMaxLength(32);
                entity.Property(e => e.DueDate).HasMaxLength(32);
                entity.Property(e => e.ReturnDate).HasMaxLength(32);
                entity.Property(e => e.CoverImagePath).HasMaxLength(1024);
                entity.Property(e => e.AvatarBg).HasMaxLength(16);
                entity.Property(e => e.AvatarFg).HasMaxLength(16);
                entity.Property(e => e.FineAmount).HasPrecision(18, 2);
                entity.Property(e => e.PaidFineAmount).HasPrecision(18, 2);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(32);
                entity.Ignore(e => e.FinePaid);
                entity.Ignore(e => e.OutstandingFine);
                entity.Ignore(e => e.StatusText);
                entity.Ignore(e => e.StatusBg);
                entity.Ignore(e => e.StatusFg);
                entity.Ignore(e => e.StatusDot);
                entity.Ignore(e => e.HasFine);
                entity.Ignore(e => e.CanCollectFine);
                entity.Ignore(e => e.FineAmountDisplay);
            });

            modelBuilder.Entity<PaymentRecord>(entity =>
            {
                entity.ToTable("Payments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(64);
                entity.Property(e => e.BorrowId).HasMaxLength(64);
                entity.Property(e => e.BorrowMaPhieu).HasMaxLength(64);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.PaidDate).HasMaxLength(32);
                entity.Property(e => e.Note).HasMaxLength(512);
                entity.Property(e => e.ReceiptNumber).HasMaxLength(64);
                entity.Ignore(e => e.AmountDisplay);
            });
        }
    }
}
