using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Domain.Enums;
using FluentValidation;

namespace Forklaringsmodell.Application.Validators;

/// <summary>
/// Håndhever regel 3.2 (skjønn må alltid forklares: Hovedhensyn er obligatorisk når
/// Type == Skjonn) og regel 3.3 (Konfidens skal være mellom 0 og 1, og bør kun være satt
/// når Type == GenerativKI eller en annen statistisk vurderingstype).
/// </summary>
public class OpprettVurderingDtoValidator : AbstractValidator<OpprettVurderingDto>
{
    public OpprettVurderingDtoValidator()
    {
        RuleFor(x => x.RegelId).NotEmpty();

        // Regel 3.14: en Vurdering-rad skal opprettes selv når vilkåret ikke faktisk ble
        // vurdert, og Utfall skiller reell manglende vurdering fra reelle konklusjoner —
        // feltet er derfor obligatorisk, ikke bare et nullable "hvis du vil".
        RuleFor(x => x.Utfall)
            .NotNull()
            .WithMessage("Utfall er obligatorisk.");

        RuleFor(x => x.Hovedhensyn)
            .NotEmpty()
            .WithMessage("Hovedhensyn er obligatorisk når Vurdering.Type == Skjonn.")
            .When(x => x.Type == VurderingsType.Skjonn);

        // Regel 3.17: flerspråklig tekst — ingen duplikate språkkoder, og hver variant må
        // ha både språkkode og verdi utfylt.
        RuleFor(x => x.Hovedhensyn)
            .Must(TekstVariantDtoValidator.IngenDuplikateSpraakkoder)
            .WithMessage("Hovedhensyn kan ikke ha flere varianter for samme språk.");
        RuleForEach(x => x.Hovedhensyn).SetValidator(new TekstVariantDtoValidator());

        RuleFor(x => x.ForkastedeUtfall)
            .Must(TekstVariantDtoValidator.IngenDuplikateSpraakkoder)
            .WithMessage("ForkastedeUtfall kan ikke ha flere varianter for samme språk.");
        RuleForEach(x => x.ForkastedeUtfall).SetValidator(new TekstVariantDtoValidator());

        RuleFor(x => x.Konfidens)
            .InclusiveBetween(0m, 1m)
            .WithMessage("Konfidens skal være mellom 0 og 1.")
            .When(x => x.Konfidens.HasValue);

        RuleFor(x => x.Konfidens)
            .Null()
            .WithMessage("Konfidens bør kun være satt når Type == GenerativKI.")
            .When(x => x.Type != VurderingsType.GenerativKI);
    }
}
