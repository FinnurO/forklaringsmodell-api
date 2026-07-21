namespace Forklaringsmodell.Domain.Entities;

public class Forklaringslogg
{
    public Guid LoggId { get; set; }
    public Guid VedtakId { get; set; }

    public Vedtak? Vedtak { get; set; }
    public ICollection<ForklaringsloggOppforing> Oppforinger { get; set; } = new List<ForklaringsloggOppforing>();
}
