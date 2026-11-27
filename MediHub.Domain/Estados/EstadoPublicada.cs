using System;
using System.Linq;
using MediHub.Domain.Entities;
using MediHub.Domain.Enums;

namespace MediHub.Domain.Estados
{
    // Visible para los proveedores en el portal: recibe ofertas hasta la fecha de cierre.
    public sealed class EstadoPublicada : EstadoLicitacionBase
    {
        public override EstadoLicitacion Tipo => EstadoLicitacion.Publicada;
        public override string Nombre => "Publicada";

        public override bool PuedeIniciarEvaluacion => true;
        public override bool PuedeCerrar => true;
        public override bool PermiteSincronizarOfertas => true;
        public override bool PermiteRecibirOfertas => true;

        public override void IniciarEvaluacion(Licitacion licitacion, DateTime ahora)
        {
            // Las ofertas son ciegas hasta el cierre, así que no se evalúan antes.
            if (ahora < licitacion.FechaDeCierre)
                throw new InvalidOperationException(
                    "La recepción de ofertas sigue abierta hasta el " + licitacion.FechaDeCierre.ToString("dd/MM/yyyy HH:mm") + ".");
            if (!licitacion.Ofertas.Any())
                throw new InvalidOperationException(
                    "No se puede pasar a evaluación una licitación que no tiene ofertas.");

            CambiarA(licitacion, EstadoLicitacion.EnEvaluacion, ahora);
        }

        // Cerrar una licitación publicada la declara desierta.
        public override void Cerrar(Licitacion licitacion, DateTime ahora)
        {
            CambiarA(licitacion, EstadoLicitacion.Cerrada, ahora);
        }
    }
}
