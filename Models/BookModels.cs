namespace LibraryManagementFE.Models
{
    public enum BookAvailability
    {
        SanCo,
        DangMuon
    }

    public class BookRecord
    {
        public string Id { get; set; } = string.Empty;
        public int Stt { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string CategoryLine1 { get; set; } = string.Empty;
        public string CategoryLine2 { get; set; } = string.Empty;
        public string CoverImagePath { get; set; } = string.Empty;
        public DateTime AddedDate { get; set; } = DateTime.Now;

        /// <summary>Pill background (#EFF6FF CNTT, #FAF5FF tím…)</summary>
        public string CategoryPillBg { get; set; } = "#EFF6FF";

        /// <summary>Pill text (#1978E5, #9333EA…)</summary>
        public string CategoryPillFg { get; set; } = "#1978E5";

        public int Year { get; set; }

        public BookAvailability Availability { get; set; }

        public string StatusText => Availability == BookAvailability.SanCo ? "Sẵn có" : "Đang mượn";

        public string StatusDotColor => Availability == BookAvailability.SanCo ? "#10B981" : "#F97316";

        public string StatusTextColor => Availability == BookAvailability.SanCo ? "#047857" : "#C2410C";

        /// <summary>Cover placeholder initials (short)</summary>
        public string CoverInitials => Title.Length > 0 ? Title[..Math.Min(Title.Length, 2)].ToUpperInvariant() : "?";

        public bool HasCoverImage => !string.IsNullOrWhiteSpace(CoverImagePath);
    }
}
