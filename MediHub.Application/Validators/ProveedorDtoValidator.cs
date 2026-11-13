using FluentValidation;
using MediHub.Application.DTOs;

namespace MediHub.Application.Validators
{
    public class ProveedorDtoValidator : AbstractValidator<ProveedorDto>
    {
        public ProveedorDtoValidator()
        {
            RuleFor(p => p.Nombre)
                .NotEmpty().WithMessage("La razón social es obligatoria.")
                .MaximumLength(150).WithMessage("La razón social admite máximo 150 caracteres.");

            RuleFor(p => p.Identificacion)
                .NotEmpty().WithMessage("La cédula es obligatoria.")
                .Matches(@"^\d{9,12}$").WithMessage("La cédula debe tener entre 9 y 12 dígitos, sin guiones.");

            RuleFor(p => p.CorreoElectronico)
                .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
                .EmailAddress().WithMessage("El correo electrónico no es válido.")
                .MaximumLength(200).WithMessage("El correo admite máximo 200 caracteres.");

            RuleFor(p => p.Telefono)
                .NotEmpty().WithMessage("El teléfono es obligatorio.")
                .Matches(@"^\d{8}$").WithMessage("El teléfono debe tener 8 dígitos.");
        }
    }
}
