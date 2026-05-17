namespace LibraryManagementFE.Policies
{
    public sealed class LibraryPolicy
    {
        public int MinAge { get; set; }
        public int MaxBooksPerReader { get; set; }
        public int MaxLoanDays { get; set; }
        public int MaxRenewals { get; set; }
        public bool NotBorrowWhenCardLocked { get; set; }
        public decimal PenaltyPerDay { get; set; }
        public bool AutoLockWhenLateReturn { get; set; }

        public static LibraryPolicy Default() => new LibraryPolicy
        {
            MinAge = 16,
            MaxBooksPerReader = 5,
            MaxLoanDays = 14,
            MaxRenewals = 2,
            NotBorrowWhenCardLocked = true,
            PenaltyPerDay = 1000,
            AutoLockWhenLateReturn = true,
        };
    }
}
