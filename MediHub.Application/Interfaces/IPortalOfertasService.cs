using System;
using System.Collections.Generic;
using System.IO;
using MediHub.Application.Common;
using MediHub.Application.DTOs;

namespace MediHub.Application.Interfaces
{
    public interface IPortalOfertasService
    {
        IReadOnlyList<LicitacionVigenteDto> ObtenerVigentes(Guid idNetUser);
        PortalProveedorDto ObtenerPortal(int idLicitacion, Guid idNetUser);
        Stream AbrirPdf(int idLicitacion, Guid idNetUser);
        Resultado EnviarOferta(int idLicitacion, Guid idNetUser, int plazoEntregaDias,
            string descripcion, IReadOnlyList<DetalleOfertaDto> detalles);
        ComparacionOfertasDto Comparar(int idLicitacion);
        Resultado Adjudicar(int idLicitacion, int idOferta, Guid idComprador);
        Resultado Sincronizar(int idLicitacion, int idOferta);
    }
}
