using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Domain.Entities;

public class Sak
{
    public Guid SakId { get; set; }
    public string Tittel { get; set; } = string.Empty;
    public SakStatus Status { get; set; }
    public DateTimeOffset Opprettet { get; set; }
    public DateTimeOffset SistEndret { get; set; }
    public string? TjenesteReferanse { get; set; } // valgfri URI til CPSV-AP PublicService i data.norge.no

    public ICollection<Faktum> Faktum { get; set; } = new List<Faktum>();
    public ICollection<Partsmedvirkning> Partsmedvirkninger { get; set; } = new List<Partsmedvirkning>();
    public ICollection<Vurdering> Vurderinger { get; set; } = new List<Vurdering>();
    public ICollection<Vedtak> Vedtak { get; set; } = new List<Vedtak>();
}
