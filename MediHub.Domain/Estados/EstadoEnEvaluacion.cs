using System;
using System.Linq;
using MediHub.Domain.Entities;
using MediHub.Domain.Enums;

namespace MediHub.Domain.Estados
{
    // Ya no se reciben ofertas nuevas; se revisan las existentes para elegir la ganadora.
    public sealed class EstadoEnEvaluacion : EstadoLicitacionBase
    {
        public override EstadoLicitacion Tipo => EstadoLicitacion.EnEvaluacion;
        public override string Nombre => "En evaluación";

        public override bool PuedeAdjudicar => true;
        public override bool PuedeCerrar => true;
        public override bool PermiteSincronizarOfertas => true;

        public override Oferta Adjudicar(Licitacion licitacion, int idOferta, DateTime ahora)
        {
            var oferta = licitacion.Ofertas.SingleOrDefault(o => o.IdOferta == idOferta);
            if (oferta == null)
                throw new InvalidOperationException("La oferta no pertenece a esta licitación.");
            if (!oferta.Sincronizada)
                throw new InvalidOperationException("Solo se pueden adjudicar ofertas sincronizadas.");

            licitacion.IdOfertaAdjudicada = oferta.IdOferta;
            CambiarA(licitacion, EstadoLicitacion.Adjudicada, ahora);
            return oferta;
        }

        // Cerrar sin adjudicar declara la licitación desierta.
        public override void Cerrar(Licitacion licitacion, DateTime ahora)
        {
            CambiarA(licitacion, EstadoLicitacion.Cerrada, ahora);
        }
    }
}
