using FluentValidation;
using MediHub.Application.DTOs;

namespace MediHub.Application.Validators
{
    public class UsuarioDtoValidator : AbstractValidator<UsuarioDto>
    {
        public UsuarioDtoValidator()
        {
            RuleFor(u => u.IdProveedor)
                .GreaterThan(0).WithMessage("Seleccione el proveedor al que pertenece el usuario.");

            RuleFor(u => u.Nombres)
                .NotEmpty().WithMessage("Los nombres son obligatorios.")
                .MaximumLength(100).WithMessage("Los nombres admiten máximo 100 caracteres.");

            RuleFor(u => u.PrimerApellido)
                .NotEmpty().WithMessage("El primer apellido es obligatorio.")
                .MaximumLength(100).WithMessage("El primer apellido admite máximo 100 caracteres.");

            RuleFor(u => u.SegundoApellido)
                .NotEmpty().WithMessage("El segundo apellido es obligatorio.")
                .MaximumLength(100).WithMessage("El segundo apellido admite máximo 100 caracteres.");

            RuleFor(u => u.Identificacion)
                .NotEmpty().WithMessage("La identificación es obligatoria.")
                .Matches(@"^\d{9,10}$").WithMessage("La identificación debe tener 9 o 10 dígitos, sin guiones.");

            RuleFor(u => u.CorreoElectronico)
                .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
                .EmailAddress().WithMessage("El correo electrónico no es válido.")
                .MaximumLength(200).WithMessage("El correo admite máximo 200 caracteres.");
        }
    }
}
