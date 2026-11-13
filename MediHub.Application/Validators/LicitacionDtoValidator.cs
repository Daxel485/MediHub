using FluentValidation;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;

namespace MediHub.Application.Validators
{
    public class LicitacionDtoValidator : AbstractValidator<LicitacionDto>
    {
        public LicitacionDtoValidator(IClock clock)
        {
            RuleFor(l => l.Titulo)
                .NotEmpty().WithMessage("El título es obligatorio.")
                .MaximumLength(150).WithMessage("El título admite máximo 150 caracteres.");

            RuleFor(l => l.Descripcion)
                .MaximumLength(500).WithMessage("La descripción admite máximo 500 caracteres.");

            RuleFor(l => l.FechaDeCierre)
                .Must(fecha => fecha > clock.Ahora)
                .WithMessage("La fecha de cierre debe ser posterior a la fecha actual.");
        }
    }
}
