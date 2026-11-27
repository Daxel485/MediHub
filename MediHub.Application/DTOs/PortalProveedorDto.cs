using System;
using System.Collections.Generic;

namespace MediHub.Application.DTOs
{
    public class LicitacionVigenteDto
    {
        public int IdLicitacion { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaDeCierre { get; set; }
    }

    public class ItemParaOfertarDto
    {
        public int? IdItemLicitacion { get; set; }
        public string Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public string Unidad { get; set; }
        public int VidaUtilMinimaMeses { get; set; }
        public bool RequiereCadenaDeFrio { get; set; }
    }

    public class PortalProveedorDto
    {
        public LicitacionVigenteDto Licitacion { get; set; }
        public IReadOnlyList<ItemParaOfertarDto> Items { get; set; }
        public bool TienePdf { get; set; }
        public bool TieneOfertaPropia { get; set; }
    }

    public class DetalleOfertaDto
    {
        public int? IdItemLicitacion { get; set; }
        public string DescripcionItem { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public DateTime FechaVencimientoLote { get; set; }
        public decimal? Puntaje { get; set; }
    }
}
