using System;
using System.Collections.Generic;

namespace MediHub.Application.DTOs
{
    public class LicitacionDto
    {
        public int IdLicitacion { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaDeCierre { get; set; }
        public DateTime FechaDeRegistro { get; set; }
        public bool Estado { get; set; }
        public int CantidadDeOfertas { get; set; }
        public int OfertasPendientes { get; set; }
        public string RutaTerminos { get; set; }
        public IList<ItemLicitacionDto> Items { get; set; } = new List<ItemLicitacionDto>();
    }
}
