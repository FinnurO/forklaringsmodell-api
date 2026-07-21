using FluentValidation.Results;

namespace Forklaringsmodell.Application.Exceptions;

/// <summary>
/// Wrapper rundt FluentValidation-feil slik at Application-laget kan kaste ett konsistent
/// exception-type uavhengig av om ASP.NET Core sin FluentValidation-autovalidering er i
/// bruk. Kun FluentValidation "core"-pakken er installert i dette repoet (ingen
/// FluentValidation.AspNetCore/AspNetCore.Http-adapter), så validatorer kalles manuelt fra
/// service-laget og feilene pakkes inn her. Mappes til 400 Bad Request i Api-laget.
/// </summary>
public class AppValidationException : Exception
{
    public IReadOnlyCollection<ValidationFailure> Errors { get; }

    public AppValidationException(IEnumerable<ValidationFailure> errors)
        : base("Validering feilet.")
    {
        Errors = errors.ToList();
    }
}
