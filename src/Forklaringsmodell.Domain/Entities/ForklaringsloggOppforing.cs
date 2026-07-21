using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Domain.Entities;

public class ForklaringsloggOppforing
{
    public Guid OppforingId { get; set; }
    public Guid LoggId { get; set; }
    public OppforingsType Type { get; set; }
    public Guid ReferanseId { get; set; }

    public Forklaringslogg? Forklaringslogg { get; set; }
}
