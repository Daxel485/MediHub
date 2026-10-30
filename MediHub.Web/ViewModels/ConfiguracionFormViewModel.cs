using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MediHub.Web.ViewModels
{
    public class ConfiguracionFormViewModel
    {
        public int IdConfiguracion { get; set; }

        [Display(Name = "Proveedor")]
        public int? IdProveedor { get; set; }

        public string NombreProveedor { get; set; }

        [Display(Name = "Tipo de configuración")]
        public int? TipoConfiguracion { get; set; }

        [Display(Name = "Comisión (%)")]
        public int? Comision { get; set; }

        [Display(Name = "Activo")]
        public bool Estado { get; set; }

        public IEnumerable<SelectListItem> Proveedores { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Tipos { get; set; } = new List<SelectListItem>();
    }
}
