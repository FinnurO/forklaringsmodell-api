namespace Forklaringsmodell.Application.Dtos;

/// <summary>
/// En vurdering med sine delvurderinger nøstet inn (regel 3.18). Arver alle feltene fra
/// <see cref="VurderingDto"/>, slik at en visning kan gå rett fra tre til skjerm uten å
/// slå opp id-er i den flate listen.
/// </summary>
public class VurderingNodeDto : VurderingDto
{
    public List<VurderingNodeDto> Delvurderinger { get; set; } = new();
}

/// <summary>
/// Referansedata som forklaringen peker til, oppløst slik at klienten slipper egne oppslag.
/// Inneholder kun det som faktisk er referert fra faktum, vurderinger og virkninger i svaret
/// (pluss rettskildene som kilder, regler og vilkår selv peker til).
/// </summary>
public class ReferansedataDto
{
    public List<KildeDto> Kilder { get; set; } = new();
    public List<RettskildeDto> Rettskilder { get; set; } = new();
    public List<RegelDto> Regler { get; set; } = new();
    public List<VilkarDto> Vilkar { get; set; } = new();
}

/// <summary>Ett vedtak med forklaringslogg og virkninger, brukt i sak-forklaringen.</summary>
public class VedtakMedVirkningerDto
{
    public VedtakDto Vedtak { get; set; } = null!;
    public ForklaringsloggDto Forklaringslogg { get; set; } = null!;
    public List<VedtaksvirkningDto> Virkninger { get; set; } = new();
}

/// <summary>Rene tellinger beregnet fra innholdet i sak-forklaringen, til bruk i en oversikt.</summary>
public class SakOppsummeringDto
{
    public bool HarVedtak { get; set; }
    public int AntallVedtak { get; set; }
    public int AntallFaktum { get; set; }
    public int AntallVurderinger { get; set; }
    public int AntallEskalerteVurderinger { get; set; }
}

/// <summary>
/// Hele saken samlet i ett svar (GET /api/saker/{sakId}/forklaring): levende visning av alt
/// som hører til saken, også uten vedtak. <c>ErLaast</c> på hver rad viser hva som er frosset
/// av et vedtak. Vurderingene finnes både flatt (<see cref="Vurderinger"/>) og nøstet
/// (<see cref="Vurderingstre"/>); det er de samme radene.
/// </summary>
public class SakForklaringDto
{
    public SakDto Sak { get; set; } = null!;
    public SakOppsummeringDto Oppsummering { get; set; } = new();
    public List<SakRelasjonDto> Relasjoner { get; set; } = new();
    public List<FaktumDto> Faktum { get; set; } = new();
    public List<VurderingDto> Vurderinger { get; set; } = new();
    public List<VurderingNodeDto> Vurderingstre { get; set; } = new();
    public List<PartsmedvirkningDto> Partsmedvirkninger { get; set; } = new();
    public List<VedtakMedVirkningerDto> Vedtak { get; set; } = new();

    /// <summary>Vurderinger i andre saker som vurderinger her bygger på (regel 3.11). Følges ett nivå.</summary>
    public List<VurderingDto> RefererteVurderinger { get; set; } = new();

    /// <summary>Faktum som er referert fra vurderinger eller virkninger her, men tilhører en annen sak.</summary>
    public List<FaktumDto> AndreFaktum { get; set; } = new();

    public ReferansedataDto Referansedata { get; set; } = new();
}
