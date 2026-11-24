using Npgsql;

namespace ItOpsMonitor.Infrastructure.Data;

/// <summary>
/// Render видає адресу БД у форматі URL (postgresql://user:password@host:port/db),
/// а Npgsql очікує формат «ключ=значення». Метод перетворює URL і не змінює інші рядки.
/// </summary>
public static class ConnectionStringResolver
{
    public static string Normalize(string connectionString, DatabaseProvider provider)
    {
        var isUrl = connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
                 || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);

        if (provider != DatabaseProvider.Postgres || !isUrl)
            return connectionString;

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,          // внутрішня адреса Render — без порту
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : null,
            // Зовнішня адреса (з доменом) вимагає TLS; внутрішня (dpg-…-a) працює і без нього
            SslMode = uri.Host.Contains('.') ? SslMode.Require : SslMode.Prefer
        };

        return builder.ConnectionString;
    }
}
