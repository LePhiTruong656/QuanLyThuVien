namespace LibraryManagementFE.Services
{
    internal static class SocialAccountHelper
    {
        public static string PasswordSeed(string email) => $"oauth:{email.Trim().ToLowerInvariant()}";
    }
}
