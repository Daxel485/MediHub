using System;
using System.Collections.Generic;
using MediHub.Domain.Enums;
using MediHub.Domain.Estados;

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
        public EstadoLicitacion EstadoDelProceso { get; set; } = EstadoLicitacion.Borrador;

        public virtual ICollection<Oferta> Ofertas { get; set; } = new List<Oferta>();

        // Objeto de estado (patrón State) que corresponde al valor persistido. No se mapea a la base.
        public EstadoLicitacionBase EstadoActual => EstadoLicitacionBase.Para(EstadoDelProceso);

        // Las transiciones se delegan al estado actual, que valida si están permitidas.
        public void Publicar(DateTime ahora)
        {
            EstadoActual.Publicar(this, ahora);
        }

        public void IniciarEvaluacion(DateTime ahora)
        {
            EstadoActual.IniciarEvaluacion(this, ahora);
        }

        // La oferta ganadora se registra aparte, en la tabla Adjudicacion.
        public void Adjudicar(Oferta oferta, DateTime ahora)
        {
            EstadoActual.Adjudicar(this, oferta, ahora);
        }

        public void Cerrar(DateTime ahora)
        {
            EstadoActual.Cerrar(this, ahora);
        }
    }
}
