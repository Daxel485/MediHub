using System;

namespace MediHub.Application.DTOs
{
    public class OfertaDto
    {
        public int IdOferta { get; set; }
        public int IdLicitacion { get; set; }
        public string NombreProveedor { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaDeEnvio { get; set; }
        public bool Sincronizada { get; set; }
    }
}
