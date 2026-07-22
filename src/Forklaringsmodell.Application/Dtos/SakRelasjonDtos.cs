using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Application.Dtos;

public class SakRelasjonDto
{
    public Guid RelasjonId { get; set; }
    public Guid SakId { get; set; }
    public Guid RelatertSakId { get; set; }
    public SakRelasjonType Type { get; set; }
}

public class OpprettSakRelasjonDto
{
    public Guid RelatertSakId { get; set; }
    public SakRelasjonType Type { get; set; }
}
