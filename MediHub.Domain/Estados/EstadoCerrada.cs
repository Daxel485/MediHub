using MediHub.Domain.Enums;

namespace MediHub.Domain.Estados
{
    // Estado final: no admite ninguna transición.
    public sealed class EstadoCerrada : EstadoLicitacionBase
    {
        public override EstadoLicitacion Tipo => EstadoLicitacion.Cerrada;
        public override string Nombre => "Cerrada";
    }
}
