using Forklaringsmodell.Application.Dtos;
using FluentValidation;

namespace Forklaringsmodell.Application.Validators;

public class OpprettSakDtoValidator : AbstractValidator<OpprettSakDto>
{
    public OpprettSakDtoValidator()
    {
        RuleFor(x => x.Tittel).NotEmpty();
    }
}

public class OppdaterSakDtoValidator : AbstractValidator<OppdaterSakDto>
{
    public OppdaterSakDtoValidator()
    {
        RuleFor(x => x.Tittel).NotEmpty();
    }
}
