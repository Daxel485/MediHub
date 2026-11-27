using System;
using System.Collections.Generic;

namespace MediHub.Domain.Entities
{
    public class Oferta
    {
        public int IdOferta { get; set; }
        public int IdLicitacion { get; set; }
        public int IdProveedor { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaDeEnvio { get; set; }
        public bool Sincronizada { get; set; }

        public int? PlazoEntregaDias { get; set; }
        public virtual ICollection<DetalleOferta> Detalles { get; set; } = new List<DetalleOferta>();

        public virtual Licitacion Licitacion { get; set; }
        public virtual Proveedor Proveedor { get; set; }

        public void Sincronizar()
        {
            if (Sincronizada)
                throw new InvalidOperationException("La oferta ya se encuentra sincronizada.");
            Sincronizada = true;
        }
    }
}
