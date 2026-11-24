namespace ItOpsMonitor.Infrastructure.Data;

/// <summary>СУБД, з якою працює застосунок. Значення задається в конфігурації (Database:Provider).</summary>
public enum DatabaseProvider
{
    Sqlite,     // локальна розробка та тести (лабораторна №6)
    Postgres    // PostgreSQL на Render
}
