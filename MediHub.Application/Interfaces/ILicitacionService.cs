using System.Collections.Generic;
using MediHub.Application.Common;
using MediHub.Application.DTOs;

namespace MediHub.Application.Interfaces
{
    public interface ILicitacionService
    {
        IReadOnlyList<LicitacionDto> ObtenerTodas();
        LicitacionDto ObtenerPorId(int idLicitacion);
        Resultado Registrar(LicitacionDto licitacion);
        IReadOnlyList<OfertaDto> ObtenerOfertas(int idLicitacion);
        Resultado SincronizarOferta(int idOferta);
    }
}
