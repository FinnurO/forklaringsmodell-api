using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Application.Dtos;

public class KildeDto
{
    public Guid KildeId { get; set; }
    public string Navn { get; set; } = string.Empty;
    public KildeType Type { get; set; }
    public bool Autoritativ { get; set; }
}

public class OpprettKildeDto
{
    public string Navn { get; set; } = string.Empty;
    public KildeType Type { get; set; }
    public bool Autoritativ { get; set; }
}
