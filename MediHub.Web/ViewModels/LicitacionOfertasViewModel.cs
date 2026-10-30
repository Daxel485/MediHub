using System.Collections.Generic;
using MediHub.Application.DTOs;

namespace MediHub.Web.ViewModels
{
    public class LicitacionOfertasViewModel
    {
        public LicitacionDto Licitacion { get; set; }
        public IReadOnlyList<OfertaDto> Ofertas { get; set; }
    }
}
