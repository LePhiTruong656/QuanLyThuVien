using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LibraryManagementFE.Models
{
    public enum CardType  { SinhVien, GiaoVien }
    public enum ReaderStatus { HoatDong, HetHan }

    public class ReaderRecord : INotifyPropertyChanged
    {
        // ── Identity ───────────────────────────────────────────
        public string Name       { get; set; } = string.Empty;
        public string Email      { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string RegDate    { get; set; } = string.Empty;

        // ── Enums ──────────────────────────────────────────────
        public CardType    CardType    { get; set; }
        public ReaderStatus Status     { get; set; }

        // ── Avatar colour (derived from Name initial) ──────────
        // Returns the initials (first letters of each word, max 2)
        public string Initials => string.Concat(
            Name.Split(' ', System.StringSplitOptions.RemoveEmptyEntries)
                .Take(2).Select(w => w[0]));

        // Colour palette indexed by first char of name
        public string AvatarBackground => _avatarBg[Math.Abs(Name.GetHashCode()) % _avatarBg.Length];
        public string AvatarForeground => _avatarFg[Math.Abs(Name.GetHashCode()) % _avatarFg.Length];
        public string AvatarBorder     => _avatarBd[Math.Abs(Name.GetHashCode()) % _avatarBd.Length];

        private static readonly string[] _avatarBg = { "#EFF6FF","#FAF5FF","#FFF7ED","#FDF2F8" };
        private static readonly string[] _avatarFg = { "#004A99","#9333EA","#EA580C","#DB2777" };
        private static readonly string[] _avatarBd = { "#BFDBFE","#F3E8FF","#FFEDD5","#FCE7F3" };

        // ── Computed display strings ───────────────────────────
        public string CardTypeText   => CardType   == CardType.SinhVien   ? "SINH VIÊN" : "GIÁO VIÊN";
        public string CardTypeBg     => CardType   == CardType.SinhVien   ? "#D1E4FF"  : "#F1F5F9";
        public string CardTypeFg     => CardType   == CardType.SinhVien   ? "#004A99"  : "#475569";
        public string StatusText     => Status     == ReaderStatus.HoatDong ? "Hoạt động" : "Hết hạn";
        public string StatusDotColor => Status     == ReaderStatus.HoatDong ? "#10B981"   : "#F59E0B";
        public string StatusTextColor=> Status     == ReaderStatus.HoatDong ? "#047857"   : "#B45309";
        public string StatusBg       => Status     == ReaderStatus.HoatDong ? "#ECFDF5"   : "#FFFBEB";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
