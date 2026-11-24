namespace ItOpsMonitor.Domain.Exceptions;

/// <summary>Дані порушують обмеження сховища (UNIQUE, FOREIGN KEY, CHECK).</summary>
public class DataIntegrityException : Exception
{
    public DataIntegrityException(string message, Exception? inner = null)
        : base(message, inner) { }
}
