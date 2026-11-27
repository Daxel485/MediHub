using System;
using System.Collections.Generic;
using MediHub.Domain.Entities;

namespace MediHub.Domain.Interfaces
{
    public interface IPortalOfertasRepository
    {
        IReadOnlyList<Licitacion> ObtenerVigentes(DateTime ahora);
        Licitacion ObtenerVigente(int idLicitacion, DateTime ahora);
        Licitacion ObtenerLicitacion(int idLicitacion);
        int? ObtenerProveedorAutorizado(Guid idNetUser);
        bool TieneOferta(int idLicitacion, int idProveedor);
        IReadOnlyList<Oferta> ObtenerOfertas(int idLicitacion);
        Oferta ObtenerOferta(int idLicitacion, int idOferta);
        int? ObtenerOfertaGanadora(int idLicitacion);
        T EjecutarEnLicitacion<T>(int idLicitacion, Func<Licitacion, T> operacion);
        void AgregarOferta(Oferta oferta);
        void AgregarAdjudicacion(Adjudicacion adjudicacion);
        void AgregarEvento(BitacoraEvento evento);
    }
}
