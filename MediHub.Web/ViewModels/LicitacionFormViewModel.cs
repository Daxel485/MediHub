using System;
using System.ComponentModel.DataAnnotations;

namespace MediHub.Web.ViewModels
{
    public class LicitacionFormViewModel
    {
        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Fecha de cierre")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FechaDeCierre { get; set; }
    }
}
