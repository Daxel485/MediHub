using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace MediHub.Web.ViewModels
{
    public class LicitacionFormViewModel
    {
        [Display(Name = "Titulo")]
        public string Titulo { get; set; }

        [Display(Name = "Descripcion")]
        public string Descripcion { get; set; }

        [Display(Name = "Fecha de cierre")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FechaDeCierre { get; set; }

        [Display(Name = "Terminos de referencia (PDF)")]
        public HttpPostedFileBase ArchivoTerminos { get; set; }
    }
}
