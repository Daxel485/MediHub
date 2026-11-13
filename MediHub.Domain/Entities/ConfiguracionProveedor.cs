using System;
using MediHub.Domain.Enums;

namespace MediHub.Domain.Entities
{
    public class ConfiguracionProveedor
    {
        public int IdConfiguracion { get; set; }
        public int IdProveedor { get; set; }
        public TipoConfiguracion TipoConfiguracion { get; set; }
        // Porcentaje entero: 20 equivale a 20 % (0.20).
        public int Comision { get; set; }
        public DateTime FechaDeRegistro { get; set; }
        public DateTime? FechaDeModificacion { get; set; }
        public bool Estado { get; set; }

        public virtual Proveedor Proveedor { get; set; }

        public decimal CalcularComision(decimal monto)
        {
            return Math.Round(monto * Comision / 100m, 2);
        }
    }
}
