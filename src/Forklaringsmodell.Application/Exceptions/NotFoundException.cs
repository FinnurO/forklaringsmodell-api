namespace Forklaringsmodell.Application.Exceptions;

/// <summary>
/// Kastes når en entitet oppgitt ved id ikke finnes. Mappes til 404 Not Found i Api-laget.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
