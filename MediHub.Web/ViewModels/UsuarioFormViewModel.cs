using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MediHub.Web.ViewModels
{
    public class UsuarioFormViewModel
    {
        public int IdUsuario { get; set; }

        [Display(Name = "Proveedor")]
        public int? IdProveedor { get; set; }

        public string NombreProveedor { get; set; }

        [Display(Name = "Nombres")]
        public string Nombres { get; set; }

        [Display(Name = "Primer apellido")]
        public string PrimerApellido { get; set; }

        [Display(Name = "Segundo apellido")]
        public string SegundoApellido { get; set; }

        [Display(Name = "Identificación")]
        public string Identificacion { get; set; }

        [Display(Name = "Correo electrónico")]
        public string CorreoElectronico { get; set; }

        [Display(Name = "Activo")]
        public bool Estado { get; set; }

        public IEnumerable<SelectListItem> Proveedores { get; set; } = new List<SelectListItem>();
    }
}
