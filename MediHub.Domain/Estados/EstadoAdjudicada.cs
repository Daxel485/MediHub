using System;
using MediHub.Domain.Entities;
using MediHub.Domain.Enums;

namespace MediHub.Domain.Estados
{
    // Ya tiene oferta ganadora; solo falta cerrar el proceso.
    public sealed class EstadoAdjudicada : EstadoLicitacionBase
    {
        public override EstadoLicitacion Tipo => EstadoLicitacion.Adjudicada;
        public override string Nombre => "Adjudicada";

        public override bool PuedeCerrar => true;

        public override void Cerrar(Licitacion licitacion, DateTime ahora)
        {
            CambiarA(licitacion, EstadoLicitacion.Cerrada, ahora);
        }
    }
}
