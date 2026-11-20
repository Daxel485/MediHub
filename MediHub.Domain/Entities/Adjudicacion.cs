using System;

namespace MediHub.Domain.Entities
{
    public class Adjudicacion
    {
        public int IdLicitacion { get; set; }
        public int IdOferta { get; set; }
        public Guid IdComprador { get; set; }
        public DateTime FechaDeAdjudicacion { get; set; }
        public virtual Licitacion Licitacion { get; set; }
        public virtual Oferta Oferta { get; set; }
    }
}
