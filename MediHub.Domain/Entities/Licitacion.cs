using System;
using System.Collections.Generic;

namespace MediHub.Domain.Entities
{
    public class Licitacion
    {
        public int IdLicitacion { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaDeCierre { get; set; }
        public DateTime FechaDeRegistro { get; set; }
        public DateTime? FechaDeModificacion { get; set; }
        public bool Estado { get; set; }

        public virtual ICollection<Oferta> Ofertas { get; set; } = new List<Oferta>();
    }
}
