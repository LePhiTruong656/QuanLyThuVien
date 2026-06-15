using LibraryManagementFE.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementFE.Services
{
    /// <summary>
    /// Đồng bộ bảng Users sau khi gỡ cập nhật social auth:
    /// - Đã có dữ liệu → giữ nguyên, chỉ xóa cột AuthProvider/ExternalId.
    /// - Bảng mới tạo và rỗng → xóa hẳn bảng Users.
    /// </summary>
    public static class UserTableSyncService
    {
        private const string AddUserTableMigration = "20260604021330_AddUserTable";
        private const string AddSocialAuthMigration = "20260612000000_AddSocialAuthToUsers";

        public static void Sync(LibraryDbContext context)
        {
            if (!context.Database.CanConnect())
                return;

            if (!TableExists(context, "Users"))
                return;

            if (!ColumnExists(context, "Users", "AuthProvider"))
                return;

            var userCount = ScalarInt(context, "SELECT COUNT(*) FROM [Users]", []);

            if (userCount > 0)
                RevertSocialAuthColumnsKeepData(context);
            else
                DropNewEmptyUsersTable(context);
        }

        private static void RevertSocialAuthColumnsKeepData(LibraryDbContext context)
        {
            var emailsWithNullPassword = ReadEmailsWithNullPassword(context);
            var hasher = new AuthService(context);

            foreach (var email in emailsWithNullPassword)
            {
                var hash = hasher.HashPassword(SocialAccountHelper.PasswordSeed(email));
                context.Database.ExecuteSqlInterpolated(
                    $"UPDATE [Users] SET [PasswordHash] = {hash} WHERE [Email] = {email}");
            }

            context.Database.ExecuteSqlRaw("""
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_AuthProvider_ExternalId' AND object_id = OBJECT_ID('Users'))
                    DROP INDEX [IX_Users_AuthProvider_ExternalId] ON [Users];

                IF COL_LENGTH('Users', 'AuthProvider') IS NOT NULL
                    ALTER TABLE [Users] DROP COLUMN [AuthProvider];

                IF COL_LENGTH('Users', 'ExternalId') IS NOT NULL
                    ALTER TABLE [Users] DROP COLUMN [ExternalId];

                IF COL_LENGTH('Users', 'PasswordHash') IS NOT NULL
                    ALTER TABLE [Users] ALTER COLUMN [PasswordHash] nvarchar(512) NOT NULL;
                """);

            RemoveMigrationRecord(context, AddSocialAuthMigration);
        }

        private static void DropNewEmptyUsersTable(LibraryDbContext context)
        {
            context.Database.ExecuteSqlRaw("""
                IF OBJECT_ID('Users', 'U') IS NOT NULL
                BEGIN
                    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_AuthProvider_ExternalId' AND object_id = OBJECT_ID('Users'))
                        DROP INDEX [IX_Users_AuthProvider_ExternalId] ON [Users];

                    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('Users'))
                        DROP INDEX [IX_Users_Email] ON [Users];

                    DROP TABLE [Users];
                END
                """);

            RemoveMigrationRecord(context, AddSocialAuthMigration);
            RemoveMigrationRecord(context, AddUserTableMigration);
        }

        private static List<string> ReadEmailsWithNullPassword(LibraryDbContext context)
        {
            var emails = new List<string>();
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT [Email] FROM [Users] WHERE [PasswordHash] IS NULL";

            if (command.Connection?.State != System.Data.ConnectionState.Open)
                command.Connection?.Open();

            using var reader = command.ExecuteReader();
            while (reader.Read())
                emails.Add(reader.GetString(0));

            return emails;
        }

        private static void RemoveMigrationRecord(LibraryDbContext context, string migrationId)
        {
            context.Database.ExecuteSqlInterpolated(
                $"DELETE FROM [__EFMigrationsHistory] WHERE [MigrationId] = {migrationId}");
        }

        private static bool TableExists(LibraryDbContext context, string tableName)
        {
            return ScalarInt(context,
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @name",
                [new SqlParameter("@name", tableName)]) > 0;
        }

        private static bool ColumnExists(LibraryDbContext context, string tableName, string columnName)
        {
            return ScalarInt(context,
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table AND COLUMN_NAME = @column",
                [new SqlParameter("@table", tableName), new SqlParameter("@column", columnName)]) > 0;
        }

        private static int ScalarInt(LibraryDbContext context, string sql, SqlParameter[] parameters)
        {
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(parameters);

            if (command.Connection?.State != System.Data.ConnectionState.Open)
                command.Connection?.Open();

            var result = command.ExecuteScalar();
            return result is null or DBNull ? 0 : Convert.ToInt32(result);
        }
    }
}
