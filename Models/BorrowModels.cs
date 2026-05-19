using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace LibraryManagementFE.Models
{
    public enum BorrowStatus
    {
        DangMuon,   // Đang mượn
        DaTraTot,   // Đã trả - tốt
        DaTraTre,   // Đã trả trễ
        QuaHan,     // Quá hạn
        ChuaTraBao, // Sắp đến hạn
    }

    public class BorrowRecord : INotifyPropertyChanged
    {
        public string Id { get; set; } = string.Empty;
        public int Stt { get; set; }
        public string MaPhieu { get; set; } = "";
        public string ReaderId { get; set; } = "";
        public string BookId { get; set; } = "";
        public string ReaderName { get; set; } = "";
        public string ReaderInitials { get; set; } = "";
        public string ReaderEmail { get; set; } = "";
        public string CardNumber { get; set; } = "";
        public string BookTitle { get; set; } = "";
        public string Author { get; set; } = "";
        public int LoanDays { get; set; }
        public int RenewalCount { get; set; }
        public string ReturnNote { get; set; } = "";
        public decimal FineAmount { get; set; }
        public bool FinePaid { get; set; }
        public string FinePaidDate { get; set; } = "";
        public string BorrowDate { get; set; } = "";
        public string DueDate { get; set; } = "";
        public string ReturnDate { get; set; } = "";
        public BorrowStatus Status { get; set; }

        // Avatar colors
        public string AvatarBg { get; set; } = "#EFF6FF";
        public string AvatarFg { get; set; } = "#1978E5";

        // Status display
        public string StatusText => Status switch
        {
            BorrowStatus.DangMuon => "Đang mượn",
            BorrowStatus.DaTraTot => "Đã trả",
            BorrowStatus.DaTraTre => "Đã trả trễ",
            BorrowStatus.QuaHan => "Quá hạn",
            BorrowStatus.ChuaTraBao => "Sắp hạn",
            _ => ""
        };

        public string StatusBg => Status switch
        {
            BorrowStatus.DangMuon => "#EFF6FF",
            BorrowStatus.DaTraTot => "#DCFCE7",
            BorrowStatus.DaTraTre => "#FDE68A",
            BorrowStatus.QuaHan => "#FEE2E2",
            BorrowStatus.ChuaTraBao => "#FFF7ED",
            _ => "#F1F5F9"
        };

        public string StatusFg => Status switch
        {
            BorrowStatus.DangMuon => "#1978E5",
            BorrowStatus.DaTraTot => "#16A34A",
            BorrowStatus.DaTraTre => "#92400E",
            BorrowStatus.QuaHan => "#DC2626",
            BorrowStatus.ChuaTraBao => "#EA580C",
            _ => "#64748B"
        };

        public string StatusDot => Status switch
        {
            BorrowStatus.DangMuon => "#1978E5",
            BorrowStatus.DaTraTot => "#16A34A",
            BorrowStatus.DaTraTre => "#92400E",
            BorrowStatus.QuaHan => "#DC2626",
            BorrowStatus.ChuaTraBao => "#EA580C",
            _ => "#94A3B8"
        };

        public bool HasFine => FineAmount > 0;
        public bool CanCollectFine => FineAmount > 0 && !FinePaid && Status == BorrowStatus.DaTraTre;
        public string FineAmountDisplay => FineAmount > 0 ? string.Format(System.Globalization.CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", FineAmount) : string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    // ── Report models ────────────────────────────────────────────────────────
    public class ReportStatCard
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public string Sub { get; set; } = "";
        public string IconBg { get; set; } = "#EFF6FF";
        public string IconFg { get; set; } = "#1978E5";
    }

    public class MonthlyBorrowPoint
    {
        public string Month { get; set; } = "";
        public int Count { get; set; }
        public double RelativeHeight { get; set; }
    }

    public class PaymentRecord
    {
        public string Id { get; set; } = string.Empty;
        public string BorrowId { get; set; } = string.Empty;
        public string BorrowMaPhieu { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaidDate { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public string ReceiptNumber { get; set; } = string.Empty;

        public string AmountDisplay => string.Format(System.Globalization.CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", Amount);
    }

    public class CategoryStat
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
        public double Percent { get; set; }
        public string Color { get; set; } = "#1978E5";
    }

    public class TopBookRecord
    {
        public int Rank { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public int Borrows { get; set; }
        public string Category { get; set; } = "";
        public string CatBg { get; set; } = "#EFF6FF";
        public string CatFg { get; set; } = "#1978E5";
    }
}
