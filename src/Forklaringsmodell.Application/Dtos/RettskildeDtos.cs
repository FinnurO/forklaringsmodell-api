namespace Forklaringsmodell.Application.Dtos;

public class RettskildeDto
{
    public Guid RettskildeId { get; set; }
    public string Paragraf { get; set; } = string.Empty;
    public DateTimeOffset VersjonDato { get; set; }
    public string EliReferanse { get; set; } = string.Empty;
}

public class OpprettRettskildeDto
{
    public string Paragraf { get; set; } = string.Empty;
    public DateTimeOffset VersjonDato { get; set; }
    public string EliReferanse { get; set; } = string.Empty;
}
