using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Application.Dtos;

public class RettskildeDto
{
    public Guid RettskildeId { get; set; }
    public RettskildeType Type { get; set; }
    public string Henvisning { get; set; } = string.Empty;
    public DateTimeOffset? VersjonDato { get; set; }
    public string? EliReferanse { get; set; }
}

public class OpprettRettskildeDto
{
    public RettskildeType Type { get; set; }
    public string Henvisning { get; set; } = string.Empty;
    public DateTimeOffset? VersjonDato { get; set; }
    public string? EliReferanse { get; set; }
}
