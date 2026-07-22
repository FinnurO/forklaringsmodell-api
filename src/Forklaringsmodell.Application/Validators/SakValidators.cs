using Forklaringsmodell.Application.Dtos;
using FluentValidation;

namespace Forklaringsmodell.Application.Validators;

public class OpprettSakDtoValidator : AbstractValidator<OpprettSakDto>
{
    public OpprettSakDtoValidator()
    {
        RuleFor(x => x.Tittel).NotEmpty();

        // Spek punkt 5: POST /api/saker krever utlosendeHendelse. DTO-feltet er nullable
        // nettopp for at denne regelen skal kunne håndheves (se kommentar på DTO-en).
        RuleFor(x => x.UtlosendeHendelse)
            .NotNull()
            .WithMessage("utlosendeHendelse er obligatorisk.");
    }
}

public class OppdaterSakDtoValidator : AbstractValidator<OppdaterSakDto>
{
    public OppdaterSakDtoValidator()
    {
        RuleFor(x => x.Tittel).NotEmpty();
    }
}
