using System;
using System.Collections.Generic;

namespace MediHub.Domain.Entities
{
    public class Proveedor
    {
        public int IdProveedor { get; set; }
        public string Nombre { get; set; }
        public string Identificacion { get; set; }
        public string CorreoElectronico { get; set; }
        public string Telefono { get; set; }
        public DateTime FechaDeRegistro { get; set; }
        public DateTime? FechaDeModificacion { get; set; }
        public bool Estado { get; set; }

        public virtual ICollection<Oferta> Ofertas { get; set; } = new List<Oferta>();
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
