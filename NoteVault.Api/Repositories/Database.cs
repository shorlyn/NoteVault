using NoteVault.Api.Models;
using SqlSugar;

namespace NoteVault.Api.Repositories;

public static class Database
{
    public static SqlSugarClient Create(IConfiguration config)
    {
        var options = config.GetSection("Database").Get<DatabaseOptions>() ?? new();
        var (connectionString, dbType) = BuildConnection(options);

        return new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = dbType,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute
        });
    }

    private static (string ConnectionString, DbType DbType) BuildConnection(DatabaseOptions options)
    {
        var provider = options.Provider.Trim();
        if (provider.Equals("MariaDB", StringComparison.OrdinalIgnoreCase) ||
            provider.Equals("MySQL", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new InvalidOperationException("Database:ConnectionString is required when Database:Provider is MariaDB or MySQL.");
            }

            return (options.ConnectionString, DbType.MySql);
        }

        if (!provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase) &&
            !provider.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unsupported database provider: {options.Provider}");
        }

        var dbPath = string.IsNullOrWhiteSpace(options.Path) ? "data/notevault.db" : options.Path;
        var fullPath = Path.GetFullPath(dbPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        return ($"Data Source={fullPath}", DbType.Sqlite);
    }

    public static async Task MigrateAndSeedAsync(this SqlSugarClient db)
    {
        db.CodeFirst.InitTables(typeof(User), typeof(Folder), typeof(Note), typeof(Tag), typeof(NoteTag), typeof(Attachment));
        if (!await db.Queryable<User>().AnyAsync())
        {
            await db.Insertable(new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                MustChangePassword = true
            }).ExecuteCommandAsync();
        }
    }
}
