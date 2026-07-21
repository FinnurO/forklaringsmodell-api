using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Domain.Entities;

public class Kilde
{
    public Guid KildeId { get; set; }
    public string Navn { get; set; } = string.Empty;
    public KildeType Type { get; set; }
    public bool Autoritativ { get; set; }

    public ICollection<Faktum> Faktum { get; set; } = new List<Faktum>();
}
