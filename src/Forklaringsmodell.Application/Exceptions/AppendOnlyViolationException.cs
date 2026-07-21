namespace Forklaringsmodell.Application.Exceptions;

/// <summary>
/// Kastes når det gjøres et forsøk på å endre eller slette en rad (Faktum, Vurdering,
/// Regel, Vedtak, Forklaringslogg) som allerede er frosset/referert i en append-only
/// forklaringsmodell (jf. spesifikasjonens regel 3.1 og 3.4). Mappes til 409 Conflict
/// i Api-laget (se ExceptionHandlingMiddleware).
/// </summary>
public class AppendOnlyViolationException : Exception
{
    public AppendOnlyViolationException(string message) : base(message)
    {
    }
}
