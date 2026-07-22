using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Application.Dtos;

public class VilkarDto
{
    public Guid VilkarId { get; set; }
    public string Navn { get; set; } = string.Empty;
    public VirkningType Type { get; set; }
    public FastsettelsesmateType Fastsettelsesmate { get; set; }
    public string? StandardTekst { get; set; }
    public List<Guid> RettskildeIder { get; set; } = new();
    public Guid? RegelId { get; set; }
    public bool ErLaast { get; set; }
}

public class OpprettVilkarDto
{
    public string Navn { get; set; } = string.Empty;
    public VirkningType Type { get; set; }
    public FastsettelsesmateType Fastsettelsesmate { get; set; }
    public string? StandardTekst { get; set; }
    public List<Guid> RettskildeIder { get; set; } = new();
    public Guid? RegelId { get; set; }
}
