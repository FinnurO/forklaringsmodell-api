using Forklaringsmodell.Application.Dtos;
using FluentValidation;

namespace Forklaringsmodell.Application.Validators;

public class OpprettKildeDtoValidator : AbstractValidator<OpprettKildeDto>
{
    public OpprettKildeDtoValidator()
    {
        RuleFor(x => x.Navn).NotEmpty();
    }
}

public class OpprettRettskildeDtoValidator : AbstractValidator<OpprettRettskildeDto>
{
    public OpprettRettskildeDtoValidator()
    {
        RuleFor(x => x.Paragraf).NotEmpty();
        RuleFor(x => x.EliReferanse).NotEmpty();
    }
}

public class OpprettRegelDtoValidator : AbstractValidator<OpprettRegelDto>
{
    public OpprettRegelDtoValidator()
    {
        RuleFor(x => x.RettskildeId).NotEmpty();
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
