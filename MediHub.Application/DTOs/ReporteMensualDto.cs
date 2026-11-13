using System;

namespace MediHub.Application.DTOs
{
    public class ReporteMensualDto
    {
        public int IdReporte { get; set; }
        public int IdProveedor { get; set; }
        public string NombreProveedor { get; set; }
        public int CantidadDeLicitaciones { get; set; }
        public decimal MontoTotalOfertado { get; set; }
        public int CantidadDeOfertas { get; set; }
        public decimal MontoTotalComision { get; set; }
        public DateTime FechaDelReporte { get; set; }
    }
}
