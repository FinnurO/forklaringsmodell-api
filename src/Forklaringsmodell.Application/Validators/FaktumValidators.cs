using Forklaringsmodell.Application.Dtos;
using FluentValidation;

namespace Forklaringsmodell.Application.Validators;

public class OpprettFaktumDtoValidator : AbstractValidator<OpprettFaktumDto>
{
    public OpprettFaktumDtoValidator()
    {
        RuleFor(x => x.KildeId).NotEmpty();
        RuleFor(x => x.Verdi).NotEmpty();
    }
}

public class TransformerFaktumDtoValidator : AbstractValidator<TransformerFaktumDto>
{
    public TransformerFaktumDtoValidator()
    {
        RuleFor(x => x.Verdi).NotEmpty();
    }
}
