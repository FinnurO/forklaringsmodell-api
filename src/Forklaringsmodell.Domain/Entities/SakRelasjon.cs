using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Domain.Entities;

/// <summary>
/// Kobler en ny/oppfølgende Sak til en relatert Sak (regel 3.11) — ingen prosessmotor,
/// bare en referanse til hvorfor/hvor saken kommer fra.
/// </summary>
public class SakRelasjon
{
    public Guid RelasjonId { get; set; }
    public Guid SakId { get; set; }         // den nye/oppfølgende saken
    public Guid RelatertSakId { get; set; } // saken den følger opp/relaterer til
    public SakRelasjonType Type { get; set; }

    public Sak? Sak { get; set; }
    public Sak? RelatertSak { get; set; }
}
