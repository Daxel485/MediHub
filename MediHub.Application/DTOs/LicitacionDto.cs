using System;
using MediHub.Domain.Enums;

namespace MediHub.Application.DTOs
{
    public class LicitacionDto
    {
        public int IdLicitacion { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaDeCierre { get; set; }
        public DateTime FechaDeRegistro { get; set; }
        public bool Estado { get; set; }
        public int CantidadDeOfertas { get; set; }
        public int OfertasPendientes { get; set; }

        // Ciclo de vida. Las acciones disponibles las decide el estado actual (patrón State).
        public EstadoLicitacion EstadoDelProceso { get; set; }
        public string EstadoDelProcesoDescripcion { get; set; }
        public bool PuedePublicar { get; set; }
        public bool PuedeIniciarEvaluacion { get; set; }
        public bool PuedeAdjudicar { get; set; }
        public bool PuedeCerrar { get; set; }
        public bool PermiteSincronizarOfertas { get; set; }
    }
}
