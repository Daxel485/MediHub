using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using MediHub.Domain.Constants;

namespace MediHub.Web.ViewModels
{
    public class RegistroViewModel : IValidatableObject
    {
        public RegistroViewModel()
        {
            Rol = Roles.Proveedor;
        }

        [Display(Name = "Tipo de cuenta")]
        public string Rol { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los nombres admiten máximo 100 caracteres.")]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "El primer apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El primer apellido admite máximo 100 caracteres.")]
        [Display(Name = "Primer apellido")]
        public string PrimerApellido { get; set; }

        [Required(ErrorMessage = "El segundo apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El segundo apellido admite máximo 100 caracteres.")]
        [Display(Name = "Segundo apellido")]
        public string SegundoApellido { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        [StringLength(200, ErrorMessage = "El correo admite máximo 200 caracteres.")]
        [Display(Name = "Correo electrónico")]
        public string CorreoElectronico { get; set; }

        // Solo para el rol Proveedor (se guardan en el registro de Usuario).
        [Display(Name = "Proveedor")]
        public int? IdProveedor { get; set; }

        [Display(Name = "Identificación")]
        public string Identificacion { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; }

        [Required(ErrorMessage = "Confirme la contraseña.")]
        [DataType(DataType.Password)]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarContrasena { get; set; }

        public IEnumerable<SelectListItem> Proveedores { get; set; } = new List<SelectListItem>();

        public bool EsProveedor
        {
            get { return Rol == Roles.Proveedor; }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Evita que se envíe a mano un rol no permitido (por ejemplo, Administrador).
            if (!Roles.DeRegistroPublico.Contains(Rol))
                yield return new ValidationResult("Seleccione un tipo de cuenta válido.", new[] { "Rol" });

            if (EsProveedor)
            {
                if (!IdProveedor.HasValue)
                    yield return new ValidationResult("Seleccione el proveedor al que pertenece.", new[] { "IdProveedor" });
                if (string.IsNullOrWhiteSpace(Identificacion))
                    yield return new ValidationResult("La identificación es obligatoria.", new[] { "Identificacion" });
            }
        }
    }
}
