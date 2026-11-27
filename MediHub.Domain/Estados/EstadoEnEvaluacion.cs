using System;
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

        public override void Adjudicar(Licitacion licitacion, Oferta oferta, DateTime ahora)
        {
            if (oferta == null || oferta.IdLicitacion != licitacion.IdLicitacion)
                throw new InvalidOperationException("La oferta no pertenece a esta licitación.");
            if (!oferta.Sincronizada)
                throw new InvalidOperationException("Solo se pueden adjudicar ofertas sincronizadas.");

            CambiarA(licitacion, EstadoLicitacion.Adjudicada, ahora);
        }

        // Cerrar sin adjudicar declara la licitación desierta.
        public override void Cerrar(Licitacion licitacion, DateTime ahora)
        {
            CambiarA(licitacion, EstadoLicitacion.Cerrada, ahora);
        }
    }
}
