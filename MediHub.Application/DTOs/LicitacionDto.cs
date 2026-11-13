using System;

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
    }
}
