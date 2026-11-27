using System.Collections.Generic;
using System.IO;
using MediHub.Domain.Entities;
using MediHub.Application.DTOs;

namespace MediHub.Application.Interfaces
{
    // Adaptador de los ítems y documentos administrados por el módulo de licitaciones.
    public interface ITerminosLicitacion
    {
        IReadOnlyList<ItemParaOfertarDto> ObtenerItems(Licitacion licitacion);
        bool TienePdf(int idLicitacion);
        Stream AbrirPdf(int idLicitacion);
    }
}
