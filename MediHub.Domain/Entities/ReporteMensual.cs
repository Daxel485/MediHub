using System;

namespace MediHub.Domain.Entities
{
    public class ReporteMensual
    {
        public int IdReporte { get; set; }
        public int IdProveedor { get; set; }
        public int CantidadDeLicitaciones { get; set; }
        public decimal MontoTotalOfertado { get; set; }
        public int CantidadDeOfertas { get; set; }
        public decimal MontoTotalComision { get; set; }
        public DateTime FechaDelReporte { get; set; }

        public virtual Proveedor Proveedor { get; set; }

        public bool EsDelMes(DateTime fecha)
        {
            return FechaDelReporte.Year == fecha.Year && FechaDelReporte.Month == fecha.Month;
        }
    }
}
