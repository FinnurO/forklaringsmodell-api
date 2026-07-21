using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Domain.Entities;

public class Faktum
{
    public Guid FaktumId { get; set; }
    public Guid SakId { get; set; }
    public Guid KildeId { get; set; }
    public FaktumType Type { get; set; }
    public StrukturType Struktur { get; set; }
    public string Verdi { get; set; } = string.Empty; // fritekst eller JSON for strukturerte fakta
    public Guid? AvledetFraFaktumId { get; set; }      // selvreferanse: transformasjonsspor
    public DateTimeOffset InnhentetTidspunkt { get; set; }

    /// <summary>
    /// Skrivebeskyttet fordi denne raden allerede er referert av en ForklaringsloggOppforing.
    /// Beregnet felt (ikke lagret i databasen) satt av repository/service-laget ved lesing
    /// slik at Application-laget kan avvise korrigeringer uten et eget round-trip-kall.
    /// </summary>
    public bool ErLaast { get; set; }

    public Sak? Sak { get; set; }
    public Kilde? Kilde { get; set; }
    public Faktum? AvledetFraFaktum { get; set; }
    public ICollection<Faktum> Avledninger { get; set; } = new List<Faktum>();
    public ICollection<VurderingFaktum> VurderingFaktum { get; set; } = new List<VurderingFaktum>();
    public ICollection<FaktumRettskilde> FaktumRettskilde { get; set; } = new List<FaktumRettskilde>();
}
