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

        // Transiciones del ciclo de vida (Borrador → Publicada → En evaluación → Adjudicada → Cerrada).
        // La adjudicación está en IPortalOfertasService.Adjudicar porque registra también al comprador.
        Resultado Publicar(int idLicitacion);
        Resultado IniciarEvaluacion(int idLicitacion);
        Resultado Cerrar(int idLicitacion);
    }
}
