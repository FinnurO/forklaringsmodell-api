using Forklaringsmodell.Application.Dtos;
using FluentValidation;

namespace Forklaringsmodell.Application.Validators;

public class OpprettKildeDtoValidator : AbstractValidator<OpprettKildeDto>
{
    public OpprettKildeDtoValidator()
    {
        RuleFor(x => x.Navn).NotEmpty();

        // Regel 3.8: en Kilde skal ha minst én tilknyttet Rettskilde (hjemmel for
        // innhenting) før den kan brukes til å registrere Faktum.
        RuleFor(x => x.RettskildeIder)
            .NotEmpty()
            .WithMessage("Kilde må ha minst én tilknyttet Rettskilde (hjemmel for innhenting).");
    }
}

public class OpprettRettskildeDtoValidator : AbstractValidator<OpprettRettskildeDto>
{
    public OpprettRettskildeDtoValidator()
    {
        RuleFor(x => x.Henvisning).NotEmpty();
    }
}

public class OpprettRegelDtoValidator : AbstractValidator<OpprettRegelDto>
{
    public OpprettRegelDtoValidator()
    {
        // Regel 3.7: Regel kobles til minst én Rettskilde (mange-til-mange).
        RuleFor(x => x.RettskildeIder)
            .NotEmpty()
            .WithMessage("Regel må ha minst én tilknyttet Rettskilde.");
        RuleFor(x => x.Teknologi).NotEmpty();
    }
}

public class OpprettPartsmedvirkningDtoValidator : AbstractValidator<OpprettPartsmedvirkningDto>
{
    public OpprettPartsmedvirkningDtoValidator()
    {
        RuleFor(x => x.Innhold).NotEmpty();
    }
}

public class OpprettVedtakDtoValidator : AbstractValidator<OpprettVedtakDto>
{
    public OpprettVedtakDtoValidator()
    {
        RuleFor(x => x.Utfall).NotEmpty();
    }
}
