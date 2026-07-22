using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Domain.Entities;

public class Sak
{
    public Guid SakId { get; set; }
    public string Tittel { get; set; } = string.Empty;
    public SakStatus Status { get; set; }
    public DateTimeOffset Opprettet { get; set; }
    public DateTimeOffset SistEndret { get; set; }
    public string? CpsvTjenesteReferanse { get; set; } // valgfri IRI til cpsvno:Service i CPSV-AP-NO
    public HendelseType UtlosendeHendelse { get; set; } // hvilken hendelse på tjenesten som utløste denne saken

    public ICollection<Faktum> Faktum { get; set; } = new List<Faktum>();
    public ICollection<Partsmedvirkning> Partsmedvirkninger { get; set; } = new List<Partsmedvirkning>();
    public ICollection<Vurdering> Vurderinger { get; set; } = new List<Vurdering>();
    public ICollection<Vedtak> Vedtak { get; set; } = new List<Vedtak>();
    public ICollection<SakRelasjon> Relasjoner { get; set; } = new List<SakRelasjon>();
}
