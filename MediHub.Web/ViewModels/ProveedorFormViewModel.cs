using System.ComponentModel.DataAnnotations;

namespace MediHub.Web.ViewModels
{
    public class ProveedorFormViewModel
    {
        public int IdProveedor { get; set; }

        [Display(Name = "Razón social")]
        public string Nombre { get; set; }

        [Display(Name = "Cédula jurídica o física")]
        public string Identificacion { get; set; }

        [Display(Name = "Correo electrónico")]
        public string CorreoElectronico { get; set; }

        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }
    }
}
