
using FluentValidation;
using Solicitudes.Application.UseCases.Requests;

namespace Solicitudes.Application.Validators;

public sealed class CreateRequestCommandValidator
    : AbstractValidator<CreateRequestCommand>
{
    public CreateRequestCommandValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("El nombre de la solicitud es obligatorio.")
            .MaximumLength(150)
            .WithMessage("El nombre no puede superar los 150 caracteres.");

        RuleFor(request => request.Payload)
            .NotEmpty()
            .WithMessage("El contenido de la solicitud es obligatorio.");

        RuleFor(request => request.Type)
            .NotEmpty()
            .WithMessage("El tipo de solicitud es obligatorio.")
            .Must(type =>
                new[] { "TEXT", "JSON" }
                    .Contains(type.Trim(), StringComparer.OrdinalIgnoreCase))
            .WithMessage("El tipo debe ser TEXT o JSON.");
    }
}
