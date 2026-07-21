using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Application.Dtos;

public class RegelDto
{
    public Guid RegelId { get; set; }
    public Guid RettskildeId { get; set; }
    public string Teknologi { get; set; } = string.Empty;
    public VurderingsType Type { get; set; }
    public bool ErLaast { get; set; }
}

public class OpprettRegelDto
{
    public Guid RettskildeId { get; set; }
    public string Teknologi { get; set; } = string.Empty;
    public VurderingsType Type { get; set; }
}
