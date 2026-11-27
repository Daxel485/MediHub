using System.Collections.Generic;
using System.IO;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;
using MediHub.Domain.Entities;

namespace MediHub.Application.Services
{
    // El avance anterior describe cada compra en una sola línea, sin catálogo de ítems.
    internal sealed class TerminosLicitacionBase : ITerminosLicitacion
    {
        public IReadOnlyList<ItemParaOfertarDto> ObtenerItems(Licitacion licitacion)
        {
            return new[]
            {
                new ItemParaOfertarDto { Descripcion = licitacion.Titulo, Cantidad = 1m, Unidad = "oferta" }
            };
        }

        public bool TienePdf(int idLicitacion) => false;
        public Stream AbrirPdf(int idLicitacion) => null;
    }
}
