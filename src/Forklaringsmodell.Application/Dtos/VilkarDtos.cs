using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Application.Dtos;

public class VilkarDto
{
    public Guid VilkarId { get; set; }
    public string Navn { get; set; } = string.Empty;
    public string? Kode { get; set; }
    public string? Kodeverk { get; set; }
    public VirkningType Type { get; set; }
    public GrunnlagsType Grunnlagstype { get; set; }
    public FastsettelsesmateType Fastsettelsesmate { get; set; }
    public string? StandardTekst { get; set; }
    public List<Guid> RettskildeIder { get; set; } = new();
    public Guid? RegelId { get; set; }
    public string? CpsvTjenesteReferanse { get; set; }
    public bool ErLaast { get; set; }
}

public class OpprettVilkarDto
{
    public string Navn { get; set; } = string.Empty;
    public string? Kode { get; set; }
    public string? Kodeverk { get; set; }
    public VirkningType Type { get; set; }

    /// <summary>
    /// Nullable for at FluentValidation faktisk kan håndheve at feltet er obligatorisk
    /// (regel 3.15) — se tilsvarende kommentar på OpprettSakDto.UtlosendeHendelse.
    /// </summary>
    public GrunnlagsType? Grunnlagstype { get; set; }
    public FastsettelsesmateType Fastsettelsesmate { get; set; }
    public string? StandardTekst { get; set; }
    public List<Guid> RettskildeIder { get; set; } = new();
    public Guid? RegelId { get; set; }
    public string? CpsvTjenesteReferanse { get; set; }
}
