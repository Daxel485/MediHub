using System.ComponentModel.DataAnnotations;

namespace MediHub.Web.ViewModels
{
    public class ItemLicitacionViewModel
    {
        public int IdItemLicitacion { get; set; }
        public int IdLicitacion { get; set; }

        [Display(Name = "Descripcion del item")]
        public string Descripcion { get; set; }

        [Display(Name = "Cantidad requerida")]
        public decimal CantidadRequerida { get; set; }

        [Display(Name = "Unidad de medida")]
        public string Unidad { get; set; }

        [Display(Name = "Vida util (meses)")]
        public int VidaUtilMeses { get; set; }

        [Display(Name = "Requiere cadena de frio")]
        public bool RequiereCadenaFrio { get; set; }
    }
}
