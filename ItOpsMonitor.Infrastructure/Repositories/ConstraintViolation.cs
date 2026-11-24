using Microsoft.Data.Sqlite;
using Npgsql;

namespace ItOpsMonitor.Infrastructure.Repositories;

/// <summary>Розпізнає порушення обмежень у SQLite і PostgreSQL.</summary>
internal static class ConstraintViolation
{
    private const string Unique = "Запис з таким значенням уже існує.";
    private const string ForeignKey = "Пов'язаний запис не існує.";
    private const string Generic = "Дані порушують обмеження бази даних.";

    public static bool TryDescribe(Exception? inner, out string message)
    {
        message = inner switch
        {
            SqliteException { SqliteErrorCode: 19 } e => e.Message.Contains("UNIQUE") ? Unique
                                                       : e.Message.Contains("FOREIGN KEY") ? ForeignKey
                                                       : Generic,
            PostgresException { SqlState: "23505" } => Unique,          // unique_violation
            PostgresException { SqlState: "23503" } => ForeignKey,      // foreign_key_violation
            PostgresException { SqlState: "23514" or "23502" } => Generic, // check / not null
            _ => string.Empty
        };

        return message.Length > 0;
    }
}
