using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Domain.Entities;

public class Vedtak
{
    public Guid VedtakId { get; set; }
    public Guid SakId { get; set; }
    public DateTimeOffset Tidspunkt { get; set; }
    public string Utfall { get; set; } = string.Empty;
    public AutomatiseringsGrad AutomatiseringsGrad { get; set; }

    public Sak? Sak { get; set; }
    public Forklaringslogg? Forklaringslogg { get; set; }
    public ICollection<Vedtaksvirkning> Virkninger { get; set; } = new List<Vedtaksvirkning>();
}
