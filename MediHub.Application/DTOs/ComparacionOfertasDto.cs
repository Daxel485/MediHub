using System.Collections.Generic;
using MediHub.Domain.Enums;

namespace MediHub.Application.DTOs
{
    public class ComparacionOfertasDto
    {
        public LicitacionVigenteDto Licitacion { get; set; }
        public bool RecepcionCerrada { get; set; }
        public bool PuedeAdjudicar { get; set; }
        public int? IdOfertaGanadora { get; set; }
        public IReadOnlyList<OfertaDto> Ofertas { get; set; }

        // Ciclo de vida: las acciones disponibles las decide el estado actual (patrón State).
        public EstadoLicitacion EstadoDelProceso { get; set; }
        public bool PuedePublicar { get; set; }
        public bool PuedeIniciarEvaluacion { get; set; }
        public bool PuedeCerrar { get; set; }
        public bool PermiteSincronizarOfertas { get; set; }
    }
}
