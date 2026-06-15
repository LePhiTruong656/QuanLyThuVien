namespace LibraryManagementFE.Services
{
    public static class AuthProviders
    {
        public const string Local = "local";
        public const string Google = "google";
        public const string Facebook = "facebook";

        public static string GetDisplayName(string provider) => provider switch
        {
            Google => "Google",
            Facebook => "Facebook",
            _ => "email"
        };
    }
}
