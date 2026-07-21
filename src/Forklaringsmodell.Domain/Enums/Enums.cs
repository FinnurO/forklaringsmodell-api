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

public enum VurderingsType
{
    Deterministisk,
    GenerativKI,
    Skjonn
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
