using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MediHub.Application.DTOs;

namespace MediHub.Web.ViewModels
{
    public class OfertaFormViewModel
    {
        [Required, Range(1, int.MaxValue), Display(Name = "Plazo de entrega (días)")]
        public int? PlazoEntregaDias { get; set; }

        [StringLength(500), Display(Name = "Descripción de la oferta")]
        public string Descripcion { get; set; }

        public List<DetalleOfertaFormViewModel> Detalles { get; set; } = new List<DetalleOfertaFormViewModel>();

        public PortalProveedorDto Portal { get; set; }
    }

    public class DetalleOfertaFormViewModel
    {
        public int? IdItemLicitacion { get; set; }

        [Required, StringLength(20)]
        public string PrecioUnitario { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime? FechaVencimientoLote { get; set; }
    }
}
