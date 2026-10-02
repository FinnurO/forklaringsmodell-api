using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Application.Dtos;

public class VurderingDto
{
    public Guid VurderingId { get; set; }
    public Guid SakId { get; set; }
    public Guid RegelId { get; set; }
    public VurderingsType Type { get; set; }
    public UtfallType Utfall { get; set; }
    public string? Beregningsspor { get; set; }
    public decimal? Konfidens { get; set; }
    public bool Eskalert { get; set; }
    public List<TekstVariantDto> Hovedhensyn { get; set; } = new();
    public List<TekstVariantDto> ForkastedeUtfall { get; set; } = new();
    public Guid? VilkarId { get; set; }
    public Guid? ForelderVurderingId { get; set; }
    public bool ErLaast { get; set; }
    public List<Guid> FaktumIder { get; set; } = new();
    public List<Guid> RettskildeIder { get; set; } = new();
    public List<Guid> RefererteVurderingIder { get; set; } = new();

    /// <summary>Innkommende: vurderinger i samme sak som har denne som ForelderVurderingId. Beregnet, ikke en del av opprettelsen.</summary>
    public List<Guid> DelvurderingIder { get; set; } = new();
}

public class OpprettVurderingDto
{
    public Guid RegelId { get; set; }
    public VurderingsType Type { get; set; }

    /// <summary>
    /// Nullable for at FluentValidation faktisk kan håndheve at feltet er obligatorisk
    /// (regel 3.14) — se tilsvarende kommentar på OpprettSakDto.UtlosendeHendelse.
    /// </summary>
    public UtfallType? Utfall { get; set; }
    public string? Beregningsspor { get; set; }
    public decimal? Konfidens { get; set; }
    public bool Eskalert { get; set; }
    public List<TekstVariantDto> Hovedhensyn { get; set; } = new();
    public List<TekstVariantDto> ForkastedeUtfall { get; set; } = new();
    public Guid? VilkarId { get; set; }
    public Guid? ForelderVurderingId { get; set; }
    public List<Guid> FaktumIder { get; set; } = new();
    public List<Guid> RettskildeIder { get; set; } = new();
    public List<Guid> RefererteVurderingIder { get; set; } = new();
}
