using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Application.Dtos;

public class SakDto
{
    public Guid SakId { get; set; }
    public string Tittel { get; set; } = string.Empty;
    public SakStatus Status { get; set; }
    public DateTimeOffset Opprettet { get; set; }
    public DateTimeOffset SistEndret { get; set; }
    public string? CpsvTjenesteReferanse { get; set; }
    public HendelseType UtlosendeHendelse { get; set; }
}

public class OpprettSakDto
{
    public string Tittel { get; set; } = string.Empty;
    public SakStatus Status { get; set; } = SakStatus.UnderBehandling;
    public string? CpsvTjenesteReferanse { get; set; }

    /// <summary>
    /// Nullable her (i motsetning til domenets ikke-nullable HendelseType) for at
    /// FluentValidation faktisk kan håndheve at feltet er obligatorisk i request-body
    /// (spek punkt 5) — en ikke-nullable enum ville alltid "bestått" NotEmpty siden
    /// default-verdien (Soknad) er en gyldig verdi.
    /// </summary>
    public HendelseType? UtlosendeHendelse { get; set; }
}

public class OppdaterSakDto
{
    public string Tittel { get; set; } = string.Empty;
    public SakStatus Status { get; set; }
}
