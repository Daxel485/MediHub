using FluentValidation;
using MediHub.Application.DTOs;

namespace MediHub.Application.Validators
{
    public class ConfiguracionProveedorDtoValidator : AbstractValidator<ConfiguracionProveedorDto>
    {
        public ConfiguracionProveedorDtoValidator()
        {
            RuleFor(c => c.IdProveedor)
                .GreaterThan(0).WithMessage("Seleccione un proveedor.");

            RuleFor(c => c.TipoConfiguracion)
                .IsInEnum().WithMessage("Seleccione un tipo de configuración válido.");

            RuleFor(c => c.Comision)
                .InclusiveBetween(0, 100).WithMessage("La comisión debe estar entre 0 y 100 %.");
        }
    }
}
