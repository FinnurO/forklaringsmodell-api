using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Application.Dtos;

public class RegelDto
{
    public Guid RegelId { get; set; }
    public List<Guid> RettskildeIder { get; set; } = new();
    public string Teknologi { get; set; } = string.Empty;
    public VurderingsType Type { get; set; }
    public string? CpsvRegelReferanse { get; set; }
    public string? RegeldefinisjonReferanse { get; set; }
    public bool ErLaast { get; set; }
}

public class OpprettRegelDto
{
    public List<Guid> RettskildeIder { get; set; } = new();
    public string Teknologi { get; set; } = string.Empty;
    public VurderingsType Type { get; set; }
    public string? CpsvRegelReferanse { get; set; }
    public string? RegeldefinisjonReferanse { get; set; }
}
