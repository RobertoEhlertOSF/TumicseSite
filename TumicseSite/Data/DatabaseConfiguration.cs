using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace TumicseSite.Data;

public static class DatabaseConfiguration
{
    private const string SqliteProviderName = "Sqlite";
    private const string SqlServerProviderName = "SqlServer";

    public static void Configure(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var provider = configuration["Database:Provider"]?.Trim();

        if (string.Equals(provider, SqliteProviderName, StringComparison.OrdinalIgnoreCase))
        {
            options.UseSqlite(PrepareSqliteConnectionString(connectionString));
            return;
        }

        if (!string.IsNullOrWhiteSpace(provider) &&
            !string.Equals(provider, SqlServerProviderName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unsupported database provider '{provider}'.");
        }

        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.CommandTimeout(180);
            sqlOptions.EnableRetryOnFailure();
        });
    }

    private static string PrepareSqliteConnectionString(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);

        if (string.IsNullOrWhiteSpace(builder.DataSource) ||
            string.Equals(builder.DataSource, ":memory:", StringComparison.OrdinalIgnoreCase))
        {
            return builder.ToString();
        }

        var dataSource = Environment.ExpandEnvironmentVariables(builder.DataSource);

        if (!Path.IsPathRooted(dataSource))
        {
            dataSource = Path.GetFullPath(dataSource);
        }

        var directory = Path.GetDirectoryName(dataSource);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        builder.DataSource = dataSource;
        return builder.ToString();
    }
}
