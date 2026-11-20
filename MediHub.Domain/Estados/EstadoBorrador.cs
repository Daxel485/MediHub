using System;
using MediHub.Domain.Entities;
using MediHub.Domain.Enums;

namespace MediHub.Domain.Estados
{
    // La licitación se está redactando: todavía no es visible para los proveedores.
    public sealed class EstadoBorrador : EstadoLicitacionBase
    {
        public override EstadoLicitacion Tipo => EstadoLicitacion.Borrador;
        public override string Nombre => "Borrador";

        public override bool PuedePublicar => true;
        public override bool PuedeCerrar => true;

        public override void Publicar(Licitacion licitacion, DateTime ahora)
        {
            if (licitacion.FechaDeCierre <= ahora)
                throw new InvalidOperationException(
                    "No se puede publicar una licitación cuya fecha de cierre ya pasó.");

            CambiarA(licitacion, EstadoLicitacion.Publicada, ahora);
        }

        // Cancelar un borrador lo cierra sin haber sido publicado.
        public override void Cerrar(Licitacion licitacion, DateTime ahora)
        {
            CambiarA(licitacion, EstadoLicitacion.Cerrada, ahora);
        }
    }
}
