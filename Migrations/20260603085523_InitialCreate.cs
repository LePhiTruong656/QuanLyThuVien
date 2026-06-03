using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementFE.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Stt = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CategoryLine1 = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CategoryLine2 = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CoverImagePath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoryPillBg = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CategoryPillFg = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Availability = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Borrows",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Stt = table.Column<int>(type: "int", nullable: false),
                    MaPhieu = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ReaderId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BookId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ReaderName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ReaderInitials = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ReaderEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BookTitle = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LoanDays = table.Column<int>(type: "int", nullable: false),
                    RenewalCount = table.Column<int>(type: "int", nullable: false),
                    ReturnNote = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    FineAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FinePaidDate = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    BorrowDate = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DueDate = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ReturnDate = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    PaidFineAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CoverImagePath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    AvatarBg = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    AvatarFg = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Borrows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BorrowId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BorrowMaPhieu = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidDate = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Readers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DateOfBirth = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RegDate = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CardType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Readers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Borrows");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Readers");
        }
    }
}
