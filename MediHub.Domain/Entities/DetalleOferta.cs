using System;

namespace MediHub.Domain.Entities
{
    public class DetalleOferta
    {
        public int IdDetalleOferta { get; set; }
        public int IdOferta { get; set; }
        // Null identifica la línea general de las licitaciones anteriores a ItemLicitacion.
        public int? IdItemLicitacion { get; set; }
        public string DescripcionItem { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public DateTime FechaVencimientoLote { get; set; }
        public decimal? Puntaje { get; set; }
    }
}
