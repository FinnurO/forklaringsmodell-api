namespace Forklaringsmodell.Domain.Enums;

public enum FaktumType
{
    Raatt,
    Subsumert
}

public enum StrukturType
{
    Strukturert,
    Ustrukturert
}

public enum KildeType
{
    AutoritativtRegister,
    Soknad,
    TredjepartsUttalelse,
    AnnenKilde
}

public enum RettskildeType
{
    Lov,
    Forskrift,
    Rundskriv,
    Forarbeider,
    Rettspraksis,
    InternasjonalRett,
    Forvaltningspraksis
}

public enum VurderingsType
{
    Deterministisk,
    GenerativKI,
    Skjonn
}

public enum UtfallType
{
    Oppfylt,
    IkkeOppfylt,
    Uaktuelt,
    IkkeVurdert,
    Uavklart
}

public enum GrunnlagsType
{
    Rettslig,
    InternPraksis,
    Datakvalitet
}

public enum AutomatiseringsGrad
{
    Helautomatisert,
    DelvisAutomatisert,
    Manuell
}

public enum PartsmedvirkningType
{
    Forhaandsvarsel,
    Kommentar,
    InnsynsKrav
}

public enum OppforingsType
{
    Faktum,
    Vurdering,
    Partsmedvirkning
}

public enum SakStatus
{
    UnderBehandling,
    Avsluttet,
    Klaget
}

public enum VirkningType
{
    Tillatelse,
    Plikt,
    OkonomiskYtelse,
    Tilskudd,
    Gebyr
}

public enum FastsettelsesmateType
{
    Statisk,
    Parametrisert,
    Skjonnsbasert,
    Avledet
}

public enum VarighetsType
{
    Tidsbegrenset,
    Varig,
    LopendeInntilVilkarBrister
}

public enum HendelseType
{
    Soknad,
    Innrapportering,
    Melding,
    Tilbakekall,
    Kontroll,
    Klage,
    Omgjoring
}

public enum SakRelasjonType
{
    Tilbakekall,
    Revurdering,
    OppfolgingAvMelding,
    Klage,
    Kontroll,
    Annet
}
