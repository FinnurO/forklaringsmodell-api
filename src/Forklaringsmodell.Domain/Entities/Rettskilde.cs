namespace Forklaringsmodell.Domain.Entities;

public class Rettskilde
{
    public Guid RettskildeId { get; set; }
    public string Paragraf { get; set; } = string.Empty;
    public DateTimeOffset VersjonDato { get; set; }
    public string EliReferanse { get; set; } = string.Empty;

    public ICollection<Regel> Regler { get; set; } = new List<Regel>();
}
