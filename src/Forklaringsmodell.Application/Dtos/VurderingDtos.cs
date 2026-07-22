using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Application.Dtos;

public class VurderingDto
{
    public Guid VurderingId { get; set; }
    public Guid SakId { get; set; }
    public Guid RegelId { get; set; }
    public VurderingsType Type { get; set; }
    public string? Beregningsspor { get; set; }
    public decimal? Konfidens { get; set; }
    public bool Eskalert { get; set; }
    public string? Hovedhensyn { get; set; }
    public string? ForkastedeUtfall { get; set; }
    public bool ErLaast { get; set; }
    public List<Guid> FaktumIder { get; set; } = new();
    public List<Guid> RettskildeIder { get; set; } = new();
    public List<Guid> RefererteVurderingIder { get; set; } = new();
}

public class OpprettVurderingDto
{
    public Guid RegelId { get; set; }
    public VurderingsType Type { get; set; }
    public string? Beregningsspor { get; set; }
    public decimal? Konfidens { get; set; }
    public bool Eskalert { get; set; }
    public string? Hovedhensyn { get; set; }
    public string? ForkastedeUtfall { get; set; }
    public List<Guid> FaktumIder { get; set; } = new();
    public List<Guid> RettskildeIder { get; set; } = new();
    public List<Guid> RefererteVurderingIder { get; set; } = new();
}
