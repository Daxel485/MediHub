using System.Collections.Generic;

namespace MediHub.Application.DTOs
{
    public class ComparacionOfertasDto
    {
        public LicitacionVigenteDto Licitacion { get; set; }
        public bool RecepcionCerrada { get; set; }
        public bool PuedeAdjudicar { get; set; }
        public int? IdOfertaGanadora { get; set; }
        public IReadOnlyList<OfertaDto> Ofertas { get; set; }
    }
}
